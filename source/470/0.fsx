

open System
open System.Text.RegularExpressions
open System.Net
open System.IO
open System.Text
open System.Runtime.Serialization


///Measures time spent in an eagerly executed function.
///Not gonna work with a lazy function, e.g. function returning a sequence (IEnumerable).
let time jobName job = 
    let startTime = DateTime.Now;
    let returnValue = job()
    let endTime = DateTime.Now;
    printfn "%s took %d ms" jobName (int((endTime - startTime).TotalMilliseconds))
    returnValue


let limitStringLength (string : string) maxLength =
    if(string.Length > maxLength) then
        string.Substring(0, maxLength-3) + "..."
    else 
        string

///Default single match regex: "." matches any character, except "\n".
let regexSingleMatch text pattern =
    Regex.Match(text, pattern).Groups.Item(1).Value

///Treats input as single line, so "." pattern mathces any character including "\n".
let regexSingleMatchSingleLine text pattern =
    Regex.Match(text, pattern, RegexOptions.Singleline).Groups.Item(1).Value

let regexMatches text pattern =
    seq { for m in Regex.Matches(text, pattern) -> m.Groups.Item(1).Value } |> Seq.toList

let regexReplace text pattern (replacement:string) =
    Regex.Replace(text, pattern, replacement)

///Removes all occurrences of the given string from the given text
let regexRemove text pattern =
    Regex.Replace(text, pattern, "")


///Merges 2 lists, calling merge function for the elements with equal keys.
///Function assumes that all keys in the second list are unique.
///Function assumes both lists are ordered ascending.
//NOTE: we specify return type for keyExtractor to prevent generic comparison to fire for keys
let rec orderedListsMerge xs ys (keyExtractor : 'a->int) merger =
    match xs, ys with
    | [],_ | _,[] -> []
    | x::xs', y::ys' ->
        let xkey = keyExtractor x
        let ykey = keyExtractor y
        if(xkey = ykey) then
            //here we move xs forward, but keep ys the same,
            //because we assume that next y have a different key while next x might still have the same key,
            //without that assumption the results are incorrect
            (merger x y) :: orderedListsMerge xs' ys keyExtractor merger
        elif(xkey > ykey) then
            orderedListsMerge xs ys' keyExtractor merger
        else
            orderedListsMerge xs' ys keyExtractor merger


///Requests HTML for the given URL using Windows-1251 encoding.
let webRequestHtmlWin1251 (url : string) =
    let req = WebRequest.Create(url)
    let resp = req.GetResponse()
    let stream = resp.GetResponseStream()
    //don't forget the Encoding, when you work with international documents
    let reader = new StreamReader(stream, Encoding.GetEncoding("Windows-1251"))     
    let html = reader.ReadToEnd()
    resp.Close()
    html

///Removes html markup from the given text
let cleanupHtml text =
    let htmlTagPattern = "<.+?>"
    regexReplace text htmlTagPattern String.Empty

///Takes first line from the given text
let takeFirstLine text =
    let firstLinePattern = "(.*)"
    regexSingleMatch text firstLinePattern


///Extract HREFS only from the named links
let extractNamedHrefs html = 
    //I tried XmlDocument here, but it doesn't work 
    //as HTML can contain some "invalid" elements like &nbsp;
    //
    //Stand back now, I'm going to use regular expressions!
    let hrefPattern = "<a name=.* href=\"(.+?)\">.*</a>"
    regexMatches html hrefPattern


type Poem(poemHref : string, title : string, lines : seq<string>) =
    let parsedLines = [ for line in lines -> line.Replace("&nbsp;" , "") ]
    let lineTokens = [ for line in lines -> regexMatches (line.ToLower()) "([а-я]+)" ]
    let parsedTitle = 
        match title with
        //Take first line as the title for untitled poems
        | "* * *" -> (if (lines |> Seq.isEmpty) then "" else (lines |> Seq.nth 0))
        | _ -> title

    member this.Href = poemHref
    member this.Lines = parsedLines
    ///Line tokens are Russian words comprising the line.    
    member this.LineTokens = lineTokens
    member this.Title = parsedTitle

    member this.SerializationInfo = (poemHref, title, lines)


//TODO: optimize to use sorted output for grouping
/// Transforms seq<key*data> to seq<key*seq<data>> ordered by key.
let extractKeySortAndGroupBy sequence =
    sequence
        |> Seq.groupBy fst
        |> Seq.sortBy fst
        |> Seq.map (fun (key, elements) -> (key, elements |> Seq.map (fun (key, data) -> data)))
    

///Builds inversed index of tokens in poems.
///Index structure is (token -> poem number -> (line number,position in line)).
let indexPoems (poems : list<Poem>) = 
    poems
        |> List.mapi 
        (
            fun poemNumber poem -> 
                poem.LineTokens 
                    |> List.mapi
                    (
                        fun lineNumber tokens ->
                            tokens
                                |> List.mapi
                                (
                                    fun position token ->
                                        //nested tokens to simplify grouping later
                                        (token, (poemNumber, (lineNumber, position)))
                                )
                    )
                    |> List.collect id
        )
        |> List.collect id

        //now we have raw list of tuples, we will turn it into ordered inversed index

        |> extractKeySortAndGroupBy
        |> Seq.map 
        (
            fun (token, tuples) ->
                let poems =
                    tuples 
                        |> extractKeySortAndGroupBy
                        |> Seq.map 
                        (
                            fun (poemNumber, tuples) ->
                                let linesPositions =
                                    tuples
                                        |> Seq.sortBy ( fun (lineNumber,position) -> position)
                                        |> Seq.sortBy ( fun (lineNumber,position) -> lineNumber)    //sortBy is stable according to MSDN
                                        |> Seq.toList
                                (poemNumber, linesPositions)
                        )
                        |> Seq.toList
                (token, poems)
        )
        |> Seq.toList

///Token index is a subtree of the given index including only poems and lines with the given token in the given position.
let tokenIndex fullIndex filterToken filterPosition =
    let (token, poems) = 
        fullIndex
            |> List.find (fun(token, poems) -> token=filterToken)
    poems
        |> List.map
        (
            fun (poemNumber, linesPositions) ->
                let filteredLines = linesPositions |> List.filter (fun (lineNumber, position) -> position = filterPosition)
                (poemNumber, filteredLines)
        )
        |> List.filter (fun (poemNumber, linesPositions) -> not (Seq.isEmpty linesPositions))



///Intersect current index with token index.
///We will only keep tokens that occur in the poems and lines of the token index.
let intersectIndex currentIndex tokenIndex =
    //function to merge lists of (lineNumber,position) from current index and token index
    let mergeLinesPositions currentLinesPositions tokenLinesPositions =
        let keyExtractor = fst
        let merger = (fun (currentLineNumber, currentPosition) (_,_) -> (currentLineNumber, currentPosition))
        orderedListsMerge currentLinesPositions tokenLinesPositions keyExtractor merger

    //function to merge lists of (poemNumber, list(lineNumber,position)) from current index and token index
    let mergePoems currentPoems tokenPoems =
        let keyExtractor = fst
        let merger = (fun (currentPoemNumber, currentLinesPositions) (_, tokenLinesPositions) -> (currentPoemNumber, mergeLinesPositions currentLinesPositions tokenLinesPositions))
        orderedListsMerge currentPoems tokenPoems keyExtractor merger
            |> List.filter (fun (poemNumber, linesPositions) -> not (List.isEmpty linesPositions))

    currentIndex
        |> List.map (fun (token, poems) -> (token, mergePoems poems tokenIndex))
        |> List.filter (fun (token, poems) -> not (List.isEmpty poems))


///The function will return at most "count" terms that appear in the "findPosition" in the given index.
let queryIndex index findPosition count =
    index 
        |> List.map
        (
            fun (token, poems) ->
                let tokenFreq = 
                    poems
                        |> List.sumBy
                        (
                            fun (_, linesPositions) ->
                                linesPositions
                                    |> List.sumBy
                                    (
                                        fun (lineNumber, position) ->
                                            if (position = findPosition) then 1 else 0
                                    )
                        )
                (token, tokenFreq)
        )
        |> Seq.filter (fun (token, tokenFreq) -> tokenFreq > 0)
        |> Seq.sortBy (fun (token, tokenFreq) -> -tokenFreq)
        // Seq.take fails if there is less than "count" elements
        |> Seq.zip [1..count] 
        |> Seq.map (fun (index, element) -> element)
        |> Seq.toList


///Acquire first poem for the given token and line position
let resolveSinglePoem index findToken findPosition =
    let (token, poems) = 
        index
            |> List.find (fun (token, poems) -> token = findToken)
    
    poems
        |> List.collect
        (
            fun (poemNumber, linesPositions) ->
                linesPositions
                    |> List.filter (fun (lineNumber, position) -> position = findPosition)
                    |> List.map (fun (lineNumber, position) -> (poemNumber, lineNumber))
        )
        |> Seq.nth 0


type QueryResult =
    //token, count
    | LineVariant       of string*int
    //poemNumber, lineNumber
    | SinglePoem    of int*int

//TODO: identical strings currently will not be resolved to their poems
///Wraps results returned by queryIndex into QueryResult type
let wrappedQueryIndex filteredIndex searchPosition count =
    queryIndex filteredIndex searchPosition count
        |> List.map 
        (
            fun (token, count) ->
                match count with
                | 1 -> SinglePoem(resolveSinglePoem filteredIndex token searchPosition)
                | _ -> LineVariant(token,count)
        )


type ExtendedResult =
    | ExtendedLineVariant       of string*int
    | ExtendedSinglePoem        of string*string*int*string

///Extends QueryResult list with information from poems list
let extendQueryResults queryResults poems =
    queryResults
        |> List.map
        (
            fun result ->
                match result with
                | LineVariant (token, count) -> ExtendedLineVariant(token,count)
                | SinglePoem (poemNumber, lineNumber) -> 
                    let (poem : Poem) = 
                        poems
                            |> Seq.nth poemNumber
                    let line =
                        poem.Lines
                            |> Seq.nth lineNumber
                    ExtendedSinglePoem(poem.Title, poem.Href, lineNumber, line)
        )



//TODO: add more structural analysis -> handle sub-titles, personas
let hrefAndHtmlToPoem poemHref poemHtml =
    let titlePattern = "<h1>(.+?)</h1>" 
    //titles can be multiline, sometimes they include sub-titles, we take only the first line
    let title = (regexSingleMatchSingleLine poemHtml titlePattern) |> cleanupHtml |> takeFirstLine

    let linePattern = "<span class=\"line.*>(.+?)</span>"
    let lines = regexMatches poemHtml linePattern |> Seq.map cleanupHtml

    new Poem(poemHref,title,lines)

///Check that the given link is a link to a final edition poem
let isFinalEditionHref (href : string) =
    not (href.Contains("03edit") || href.Contains("02misc"))

let crawlPoemsFromWeb =
    let domainUrl = "http://www.rvb.ru/pushkin/"
    let volumeUrlTemplate = domainUrl + "tocvol{0}.htm"
    let poemUrlTemplate = domainUrl + "{0}"

    //take only first 4 volumes -- they contain poems
    seq { for volumeNumber in 1..4 -> String.Format(volumeUrlTemplate, volumeNumber) }
        |> Seq.map webRequestHtmlWin1251
        
        //Poems are always referenced through named links on rvb.ru
        |> Seq.collect extractNamedHrefs

        //We only take final editions of poems to avoid massive duplication of lines
        |> Seq.filter isFinalEditionHref 

        |> Seq.map (fun href -> String.Format(poemUrlTemplate, href))

//        //uncomment for development mode -- total number of Poems is ~800
//        |> Seq.take 40

        //Request and wrap individual poems
        |> Seq.map (fun href -> (hrefAndHtmlToPoem href (webRequestHtmlWin1251 href)))

        //Empty poem is not a poem!
        //It means we crawled some prose accidentally, it might happen
        |> Seq.filter (fun poem -> not (poem.Lines |> Seq.isEmpty))

        //cache results so we don't crawl poems twice
        |> Seq.cache

let savePoemsToCache cacheFilePath poemsList =
    let stream = new FileStream(cacheFilePath, FileMode.Create)
    //Serialization of the whole poem object is redundant ans
    //it also throws StackOverflow exception, because lists are serialized as nested <tail></tail> elements!
    let serializer = new DataContractSerializer(typeof<(string*string*seq<string>) list>)
    serializer.WriteObject(stream, (poemsList |> List.map (fun (poem : Poem) -> poem.SerializationInfo)))
    stream.Flush()
    stream.Close()

let loadPoemsFromCache cacheFilePath =
    let serializer = new DataContractSerializer(typeof<(string*string*seq<string>) list>)
    let stream = new FileStream(cacheFilePath, FileMode.Open)
    let poemsList = serializer.ReadObject(stream) :?> list<string*string*seq<string>>
    stream.Close()
    poemsList |> List.map (fun poemInfo -> new Poem(poemInfo))

let crawlPoemsOrLoadFromCache = 
    let cacheFilePath = "poems.cache"
    if File.Exists(cacheFilePath) then
        printfn "Loading poems from cache"
        loadPoemsFromCache cacheFilePath
    else
        printfn "Loading poems from web"
        let poems = crawlPoemsFromWeb |> Seq.toList
        savePoemsToCache cacheFilePath poems
        poems 


type PushkinTreeNode =
    | VariantNode     of string*int*seq<PushkinTreeNode>
    | PoemNode        of string*string*int*string

let createPushkinTree pushkinPoems poemsIndex count =
    let rec createTreeLevel count currentQuery currentIndex =
        let searchPosition = List.length currentQuery
        let queryResult = wrappedQueryIndex currentIndex searchPosition count
        let prettyResult = extendQueryResults queryResult pushkinPoems
        prettyResult
            |> List.map
            (
                fun result ->
                    match result with
                    | ExtendedSinglePoem (title, href, lineNumber, line) -> PoemNode(title, href, lineNumber, line)
                    | ExtendedLineVariant (token, freq) -> VariantNode(token, freq, createTreeLevel count (currentQuery @ [token]) (intersectIndex currentIndex (tokenIndex currentIndex token searchPosition)))
            )
    createTreeLevel count [] poemsIndex

let rec pushkinTreeToNumberOfQueries pushkinTree =
    let subTreeResults = 
        pushkinTree
            |> Seq.sumBy 
            ( 
                fun pushkinTreeNode ->
                    match pushkinTreeNode with
                    | VariantNode (_, _, subTree) -> pushkinTreeToNumberOfQueries subTree
                    | PoemNode (_, _, _, _) -> 0
            )
    subTreeResults+1

let resultsToHtml pushkinTree =
    let rec treeToHtml tree currentPath (currentNumber:int) startingNumber =
        let zippedTreeLevel =
            tree
                |> Seq.zip (Seq.initInfinite (fun i -> startingNumber+i))

        let pathToString path =
            let parts = 
                path
                    |> Seq.map (fun x -> "'"+x+"'")
            String.Join(",", parts)

        let thisLevelStart = String.Format("<div class=\"x\"><table id=\"{0}\" class=\"p\">", currentNumber) + Environment.NewLine
        let thisLevelTable = 
            zippedTreeLevel
                |> Seq.fold
                (
                    fun acc treeNode ->
                        match treeNode with
                        | (number, PoemNode (title, href, lineNumber, line)) ->
                            acc + String.Format("<tr><td><span class=line>{0}</span> &#8658; <a target=blank class=fromlink href=\"{1}\">{2}</a>, ?????? {3}</tr>",line, href, limitStringLength title 30, lineNumber+1) + Environment.NewLine
                        | (number, VariantNode (token, freq, subtree)) ->
                            acc + String.Format("<tr id=\"r{0}\"><td>{1} &#8658; <span class=\"lv\" onClick=\"x([{2}])\">{3}</span></td></tr>", number, token, (pathToString (currentPath@[string(number)])), freq) + Environment.NewLine
                ) ""
        let thisLevelEnd = @"</table></div>" + Environment.NewLine + Environment.NewLine

        let thisLevelOutput = (thisLevelStart+thisLevelTable+thisLevelEnd)

        let levelLength = 
            tree
                |> Seq.length

        let (subTreeCount, subTreeOutput) =
            zippedTreeLevel 
                |> Seq.fold
                (
                    fun (acc, result) treeNode ->
                        match treeNode with
                        | (number, PoemNode (title, href, lineNumber, line)) ->
                            (acc, result)
                        | (number, VariantNode (token, freq, subtree)) ->
                            let (subTreeCount, subTreeOutput) = treeToHtml subtree (currentPath@[string(number)]) number (startingNumber+acc+levelLength)
                            (acc + subTreeCount, result + subTreeOutput)
                ) (0, "")

        (levelLength + subTreeCount, thisLevelOutput + subTreeOutput)
       
    let (_, content) = treeToHtml pushkinTree ["0"] 0 1
    content

let outputResultsToFile (content:string) =
    let templateFile = "template.htm"
    let outputFile = "output.htm"
    let templateReplacePattern = "#HERE_GOES_CONTENT#"
    let templateHtml = File.ReadAllText(templateFile)
    let resultHtml = regexReplace templateHtml templateReplacePattern content
    File.WriteAllText(outputFile, resultHtml)


let poems = time "Crawling poems" (fun() -> crawlPoemsOrLoadFromCache)
printfn "Crawled %d poems" (poems |> Seq.length)
printfn "Crawled %d lines" (poems |> Seq.sumBy ( fun poem -> poem.Lines |> Seq.length))
let poemIndex = time "Indexing poems" (fun () -> poems |> indexPoems)
printfn "Index contains %d terms" poemIndex.Length
let tree = time "Generating result tree" (fun () -> createPushkinTree poems poemIndex 20) 
printfn "Number of queries made to create tree: %d" (pushkinTreeToNumberOfQueries tree)
let htmlContent = time "Generating html content" (fun () -> resultsToHtml tree)
time "Output content" (fun() -> outputResultsToFile htmlContent)
