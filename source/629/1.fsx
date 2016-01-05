module StackOverflowCrawler
open System      // Mennään .NET-perus-stäkillä.
open System.Net  // async-webrequest-versio helppo tehdä tällä: http://fsharppowerpack.codeplex.com/
open System.IO   // string-parsinta kannattaisi tehdä tällä: http://htmlagilitypack.codeplex.com/ 
open System.Web

let fetch (url : Uri) = 
    let req = WebRequest.Create (url) :?> HttpWebRequest    
    use stream = req.GetResponse().GetResponseStream()
    use reader = new StreamReader(stream)
    reader.ReadToEnd()

let makeUrl pagetype (tags:string) = 
    new Uri("http://stackoverflow.com/" + pagetype + "/tagged/" + HttpUtility.UrlEncode(tags))
let questions, unanswered = makeUrl "questions", makeUrl "unanswered"

let sumcount (fetched:string) = 
    let startpos = (fetched.IndexOf "<div class=\"summarycount al\">")+29
    let endpos = fetched.IndexOf("</div>",startpos)
    fetched.Substring(startpos,endpos-startpos).Replace(",","") |> Double.Parse

let relatedtags (basetag:string) (fetched:string) = //lisää parsintaa...
    let rec relativepositions (links:string) (found:string list) = 
        let startpos = links.IndexOf("/questions/tagged/" + basetag + "+")
        let realpos = startpos + 19 + basetag.Length
        let endpos = links.IndexOf("\"", realpos)
        let tag = links.Substring(realpos,endpos-realpos)
        match startpos with -1 -> found | _ -> tag :: relativepositions (links.Substring realpos) found    
    relativepositions fetched [] 

type surfmode = Inclusive | Exclusive

let checktag (sm:surfmode) basetag = 
    let acceptRate, minCount = 0.02, 1000.0;
    let add = match sm with Inclusive -> "+" | Exclusive -> "+-"
    let rec surf (tags:string) (tagsToSurf:string list) =
        let fetchTotalPage = tags |> (questions >> fetch) 
        let taggedQuestions = fetchTotalPage |> sumcount
        if taggedQuestions >= minCount then
            let unasweredWithTag = unanswered >> fetch >> sumcount
            let ratio = (unasweredWithTag tags) / taggedQuestions
            let surfTheRestOfTree rest = 
                let test tag = surf (tags + add + tag) []
                List.iter test rest
            do printfn "Ratio %f and count %g with tags %s" ratio taggedQuestions tags
            match ratio with
            | r when r <= acceptRate -> do printfn "--- Accepted: %s ---" basetag
            | _ -> 
                match relatedtags tags fetchTotalPage with
                | first::rest -> 
                    do printfn "Failed. Trying %d related..." rest.Length
                    surf (tags + add + first) rest
                    surfTheRestOfTree rest
                | _ -> surfTheRestOfTree tagsToSurf

    surf (basetag.ToLower()) []
    do printfn "Everything checked."

//Interactive tests:
//questions "java"
//questions "F#"
//unanswered "java"
//let fetched = questions "java" |> fetch
//fetched |> sumcount
//fetched |> relatedtags "java"
//checktag surfmode.Exclusive "F#"
//checktag surfmode.Inclusive "flash+flex"
//checktag surfmode.Exclusive "flash+flex"
//checktag surfmode.Exclusive "java" //jauhaa ikuisuuden eikä löydä mitään?
