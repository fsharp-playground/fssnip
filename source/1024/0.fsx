module JessiTron

open System.IO

let openFile file =
  try
    Some( File.OpenText(file) )
  with
  | :? System.Exception -> None

let readFirstLine (file:StreamReader) = 
  try
    Some( file.ReadLine() )
  with
  | :? System.Exception -> None

let parseLine line =
  // The secret message is '...'
  let m = System.Text.RegularExpressions.Regex.Match(line, @".*The secret message is '([^']*)'.*") 
  if m.Success then
    Some( m.Groups.[1].Value )
  else
    None

let demoImplementation impl =
  match impl @"c:\temp\secret.txt" with
  | Some (secret) -> printfn "1.  secret: %s" secret
  | None ->          printfn "1.  %s" "No secret found."

let matchVersion file =
  match openFile file with
  | None -> None
  | Some(reader) -> match readFirstLine reader with
                    | None -> None
                    | Some(line) -> parseLine line 


let rec apply'er funclist value =
  match funclist with 
  | func :: rest -> apply'er rest (func value)
  | []           -> value

let pipelineVersion file =
  let funcs = [ openFile; readFirstLine; parseLine ]
  apply'er funcs file


demoImplementation matchVersion 
System.Console.ReadLine() |> ignore