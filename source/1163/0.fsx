// Currently, only uniformed data is supported.
// meaning lines must be seperated by line break or line feed. 
// each line must have the same format, like a1,b1,c1,d1\na2,b2,c2,d2\n; 
 
open System
open System.IO

let validate (xs : string) minlen maxlen defaultArg =
  if xs.Length >= minlen && xs.Length <= maxlen then xs
  else defaultArg

// experimenting with arg parsing.
// doesn't catch all conditions 
// - Sting.empty for a file name will exception later-
[<Struct>]
type userArgs = 
  val file   : string
  val delim  : char
  val filter : string
  val count  : int
  new (file,delim,filter,count) = 
    let file   = validate file   1 128 String.Empty
    let delim  = validate delim  1 1 ";"
    let filter = validate filter 1 128 String.Empty
    let count  = validate count  1 1 "10"
    { file=file; delim=char delim; filter=filter; count=int count }

let readFileOf (data : userArgs) = seq {
  use fs = new StreamReader(File.OpenRead data.file)
  while not fs.EndOfStream do
    let line = fs.ReadLine()
    if not <| line.StartsWith data.filter then
      yield line.Split data.delim
}

// generic map reduce poached from msdn magazine
let reduceData xs = 
  Seq.fold (fun (acc : Map<_,_>) (data, num) ->
    if   Map.containsKey data   acc then
         Map.add         data ( acc.[data] + num) acc
    else Map.add         data 1 acc)
         Map.empty xs

let maybe f x = try Some <| f x with _ -> None

// not convinced of my question \ answer stuff, but could be worse
let rec askLineChoice lines =
  let printLinesWithNumber : string seq -> unit =
    Seq.iteri (printfn "[#%d] %s")

  let questionAnswer q =
    printf "[+] %s: " q; stdin.ReadLine()

  let validateIntAnswer (answer : string) =
    match maybe int answer with
      Some d -> d >= 0 | _ -> false
      
  lines |> Seq.nth 1 |> printLinesWithNumber
  
  let answer = questionAnswer "Enter integer choice #"
  let valid  = validateIntAnswer answer
  if not valid then askLineChoice lines
  else int answer, lines

// missing entries are treated as exceptions. 
// this will slow stuff down a lot if there are many missing entries
let maybeGetLine (choice,lines) =
  let f k line =
    match maybe (Array.get line) choice with
      Some b -> b,1
    | None   ->
      printfn "[!] missing %d entry from line %d" choice k
      String.Empty, 0
      
  lines |> Seq.mapi f

let cutFileAndReduce = 
  readFileOf >> askLineChoice >> maybeGetLine >> reduceData
   
// Errorless Take, default take causes exception if take < length
// however sequences are lazy, so we only find out about
// the exception as we are evaluating it, which is messy to deal with.
// instead, since we dont care too much about if we cant print top 50 out
// of a set of 20, we just take whatever we can.
let takeOf count (xs : 'a seq) = 
  if count <= 0 then Seq.empty else  
  seq {
    use e = xs.GetEnumerator() 
    for i in 0 .. count - 1 do
      if e.MoveNext() then yield e.Current }
    
let displayTopN (data : userArgs) =
  Map.toSeq
  >> Seq.sortBy (fun (x,y) -> -y)
  >> takeOf data.count

let printTopEntries data =
  let printResults xs =
    printfn "\ncount\t\tdata"
    xs |> Seq.iter (fun (x,freq) ->
          Console.WriteLine("{0}\t-\t{1,3}",freq,x))
        
  cutFileAndReduce data
  |> displayTopN data
  |> printResults 

let exitWithError () =
  fprintfn stderr "log.exe <file> <delim> <ignoreStartChar> <count>"
  fprintfn stderr "e.g. log.exe c:\\blah.log # , 15"
  exit -1

let validateUserArgs = function
   [| file; delim; skip; count |] ->
      userArgs (file=file,delim=delim,filter=skip,count=count)
  | _                             -> exitWithError ()

[<EntryPoint>]
let main argv =
  validateUserArgs argv |> printTopEntries
  0
