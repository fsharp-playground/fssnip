(* Sample log:

2013-10-26 17:11:49$$INFO$$some text$$more text$$etc$$
2013-10-26 17:12:40$$INFO$$some text$$more text$$etc$$
2013-10-26 17:20:39$$INFO$$some text$$more text$$etc$$
Some-unimportant-info...
2013-10-26 17:20:50$$INFO$$some text$$more text$$etc$$
2013-10-27 14:21:24$$ERROR$$some text$$more text$$Operation failed: $$
System.InvalidOperationException: My error
   at MyCompany.MyClass.Method(String id)
   at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage2(MessageRpc& rpc)
   at System.ServiceModel.Dispatcher.MessageRpc.Process(Boolean isOperationContextSet)
2013-10-27 14:21:47$$INFO$$some text$$more text$$etc$$
2013-10-27 14:22:20$$ERROR$$some text$$more text$$Operation failed: $$
System.InvalidOperationException: My error
   at MyCompany.MyClass.Method(String id)
   at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage2(MessageRpc& rpc)
   at System.ServiceModel.Dispatcher.MessageRpc.Process(Boolean isOperationContextSet)
2013-10-27 14:22:45$$ERROR$$some text$$more text$$Timeout expired: $$
System.Data.SqlClient.SqlException: Timeout expired.  The timeout period...
   --- End of inner exception stack trace ---
   at System.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action wrapCloseInAction)
   at MyCompany.MyClass2.OtherMethod(String param)

*)

#if INTERACTIVE
#else
module LogReader
#endif
//Log file parser for log4Net files.

open System
open System.IO

type CollectData =
| NotIntresting
| SeekStackTraceLine of string
| SeekCompanyStackTraceLine of string * string

///Log4Net item separator
[<Literal>] 
let separator = "$$"

///Beginig for a normal line. e.g. date like 2013-10-27
[<Literal>] 
let normalLineBegin = "201"

///Company namespace in the stacktrace
[<Literal>] 
let companyStackTraceSign = "MyCompany"

///Detect error line in the log: contains this string
let errorsign = separator + "ERROR" + separator

///This will parse the lines to resultset of errors
let collectErrorInfo lines =
    let rec readLines (myLines:string list) (mode:CollectData) resultdata =

        // Add current findings to collection, just in case
        let collectedResult = 
            match mode with
            | NotIntresting -> resultdata 
            | SeekStackTraceLine(a) -> 
                let info = "No-StackTrace", "", a
                (info::resultdata)
            | SeekCompanyStackTraceLine(a, b) ->
                let info = "Non-"+companyStackTraceSign+"-StackTrace", a, b
                (info::resultdata)
                
        match myLines, mode with
        //New error-line: try to seek stacktrace, store current findings
        | h::t, _ when h.StartsWith(normalLineBegin) && h.Contains(errorsign) -> 
            readLines t (SeekStackTraceLine(h)) collectedResult
        //New info-line: dont seek stacktrace, store current findings
        | h::t, _ when h.StartsWith(normalLineBegin) && not (h.Contains(errorsign)) -> 
            readLines t NotIntresting collectedResult
                
        //New stacktrace-line: try to seek Company-stack, dont store yet
        | h::t, SeekStackTraceLine(l) when not (h.StartsWith(normalLineBegin)) ->
            readLines t (SeekCompanyStackTraceLine(h, l)) resultdata

        //New Company-stack-line: everything ok, lets store the result and continue
        | h::t, SeekCompanyStackTraceLine(a, b) when not (h.StartsWith(normalLineBegin)) && h.Contains(companyStackTraceSign) ->
            readLines t NotIntresting collectedResult

        //All the other cases: continue to next line
        | h::t, _  -> readLines t mode resultdata

        //End of file: return all with the last one
        | [], _ -> collectedResult

    readLines (lines |> Seq.toList) NotIntresting []

///Function to break-down lines by separator.
///e.g. for csv-import: by default Excel doesn't support multiple character separators
let breakLineDetails mySeq = 
    let breakLineDetail (a,b,c:string) = 
        a,b,c.Split([|separator|],StringSplitOptions.None)
    mySeq |> Seq.map breakLineDetail

let filesByPath path = Directory.EnumerateFiles(path,"*.log*",SearchOption.AllDirectories)

open System.Linq

/// This will read the file, parse it and give results.
let processFiles files =
    let result = 
        files
        |> Seq.map (fun (fileOrPath:string) -> 
            match fileOrPath with
            | path when path.EndsWith(@"\") || path.EndsWith(@"/") -> 
                    filesByPath path |> Seq.map File.ReadLines |> Seq.concat
            | file -> File.ReadLines file)
        |> Seq.concat
        |> collectErrorInfo
        |> breakLineDetails
    result.GroupBy(fun (a,b,c) -> a + " " + b).OrderByDescending(fun k -> k.Count())

///For console program, we can use this
// For good UI we would input fileName and output results
// (results is a grouped list of errors by type)
[<EntryPoint>]
let main argv = 
    let filtered = argv |> Array.filter (fun i -> not (i = ""))
    match filtered with
    | [||] -> printfn "Please input filename as argument."
    | fileNames -> 
        //let fileNames = [|@"C:\...\MyProgram.log"|]
        let results = processFiles fileNames
        for i in results
            do Console.WriteLine("Count: " + i.Count().ToString() + ", Item: " + i.Key)
    0 // return an integer exit code
