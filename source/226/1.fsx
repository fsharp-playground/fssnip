module SourceErrorPrinter = 
  
  open System
  open System.Text

  let private splitLines (input:string) = 
    let input = (string input).Replace("\r\n", "\n").Replace("\r", "\n")
    RegularExpressions.Regex.Split(input, "\n")

  let private lineNum (pad:int) (input:int) = 
    (string input).PadLeft(pad, '0')

  let private makeArrow (times:int) =
    let result = new StringBuilder(times)
    result.Insert(0, "-", times).ToString() + "^"
  
  let sprintError (above:int, below:int) (line:int, column:int) (source:string) =
    let source = source |> splitLines
    let result = ref ""

    if line <= source.Length && line > 0 then
      let text = source.[line - 1]
      let nrLength = (line+below |> string).Length
      
      // Deal with lines above the error one
      for i = 1 to above do
        let prevLine = line - i

        if prevLine >= 1 then
          let num = prevLine |> lineNum nrLength
          let text = sprintf "%s: %s\n" num source.[prevLine-1] 
          result := text + !result

      // Error line and arrow
      let arrow = nrLength + column + 1 |> makeArrow
      let num = line |> lineNum nrLength
      let text = sprintf "%s: %s\n%s\n" num text arrow
      result := !result + text

      // Deal with lines below the error one
      for i = 1 to below do
        let nextLine = line + i

        if nextLine <= source.Length then
          let num = nextLine |> lineNum nrLength
          let text = sprintf "%s: %s\n" num source.[nextLine-1] 
          result := !result + text

    !result

// Example, a piece of source code from IronJS 
let source = @"/// Adds a catch variable to the scope
let addCatchLocal name (s:S) = 
  match s |> locals |> Map.tryFind name with
  | None -> 
    s |> addLocal name None
    let local = s |> locals |> Map.find name 
    let local = local |> Local.decreaseActive
    s |> replaceLocal local

  | Some local ->
    let index = LocalIndex.New (s |> localCount) None
    let local = local |> Local.addIndex index

    s := 
      {!s with 
        LocalCount = index.Index + 1
        Locals = s |> locals |> Map.add name local}"


// We partially appli SourceErrorPrinter.sprintError with
// a tuple that is the lines to show before and after
// the errornous line
let linesBefore = 2
let linesAfter = 3
let sprintError = SourceErrorPrinter.sprintError (linesBefore, linesAfter)

// We can use our function like this to print error on line 12, column 15
// from source and then just pipe it into printfn
source |> sprintError (12, 15) |> printfn "%s"

(*Gives an error like this:

10:   | Some local ->
11:     let index = LocalIndex.New (s |> localCount) None
12:     let local = local |> Local.addIndex index
------------------^
13: 
14:     s := 
15:       {!s with 

*)