open System
open FParsec
open FParsec.CharParsers

let token p = p .>> spaces

let number = token pint32
let c s = token (skipChar s)

let rec sum s = s |> chainl1 product (choice [c '+' >>% (+); c '-' >>% (-)])
and product s = s |> chainl1 atom    (choice [c '*' >>% (*); c '/' >>% (/)])
and atom    s = s |> choice [c '(' >>. sum .>> c ')'; number]

let parser = spaces >>. sum .>> eof

[<EntryPoint>]
let main argv =
  match argv with
   | [|input|] ->
     input
     |> runParserOnString parser () "input"
     |> function
         | Success (r, (), _) -> printfn "%d" r ; 0
         | Failure (s, _, ()) -> printfn "%s" s ; 1
   | _ ->
     printfn "Calculator 'expression'"
     1
