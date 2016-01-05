open System

let (|ParseNumber|_|) text = 
  match Int32.TryParse(text) with
  | true, num -> Some(sprintf "Number: %d" num)
  | _ -> None

let (|ParseExpression|_|) (text:string) = 
  if text.StartsWith("=") then 
    Some(sprintf "Formula: %s" (text.Substring(1)))
  else 
    None

let (|Length|) (text:string) = 
  text.Length

let parseFormula text =
  match text with
  | Length 0 -> "Empty"
  | ParseNumber s -> s
  | ParseExpression s -> s
  | _ -> "Error"

parseFormula "" 
parseFormula "= 4 + 3"
parseFormula "42"
parseFormula "error"
