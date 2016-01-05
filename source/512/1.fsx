(* [omit:(declarations)] *)
open  System.Text.RegularExpressions
(* [/omit] *)

// [snippet:Parsing command-line arguments]
// parse command using regex
// if matched, return (command name, command value) as a tuple
let (|Command|_|) (s:string) =
  let r = new Regex(@"^(?:-{1,2}|\/)(?<command>\w+)[=:]*(?<value>.*)$",RegexOptions.IgnoreCase)
  let m = r.Match(s)
  if m.Success
  then 
    Some(m.Groups.["command"].Value.ToLower(), m.Groups.["value"].Value)
  else
    None

// take a sequence of argument values
// map them into a (name,value) tuple
// scan the tuple sequence and put command name into all subsequent tuples without name
// discard the initial ("","") tuple
// group tuples by name 
// convert the tuple sequence into a map of (name,value seq)
let parseArgs (args:string seq) =
  args 
  |> Seq.map (fun i -> 
                    match i with
                    | Command (n,v) -> (n,v) // command
                    | _ -> ("",i)            // data
                  )
  |> Seq.scan (fun (sn,_) (n,v) -> if n.Length>0 then (n,v) else (sn,v)) ("","")
  |> Seq.skip 1
  |> Seq.groupBy (fun (n,_) -> n)
  |> Seq.map (fun (n,s) -> (n, s |> Seq.map (fun (_,v) -> v) |> Seq.filter (fun i -> i.Length>0)))
  |> Map.ofSeq
// [/snippet]

(* [omit:(Test code)] *)
// return Some(value) if key is found, None otherwise
let (?) (m:Map<string,_>) (p:string) = 
  if Map.containsKey p m
  then Some(m.[p])
  else None

[<EntryPoint>]
let main args =
    printfn "Arguments passed to function:"
    let pArgs = args |> parseArgs
    printfn "general arguments: %A" pArgs?("")
    printfn "test: %A" pArgs?test
    printfn "show: %A" pArgs?show
    printfn "unknown: %A" pArgs?unknown
    0
(* [/omit] *)