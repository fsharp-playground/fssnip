#r "nunit.framework.dll"
open System
open NUnit.Framework

// ----------------------------------------------------------------------------

let toString chars =
  System.String(chars |> Array.ofList)

// TODO: Read until the end of inline code
let rec parseInlineBody acc chars = 
  failwith "!"

// TODO: Match beginning and read until the end
let parseInline chars = 
  failwith "!"

// ----------------------------------------------------------------------------

// Definition of markdown span element
type MarkdownSpan =
  | Literal of string
  | InlineCode of string

// Parse spans of the input
let rec parseSpans acc chars = seq {
  // emit literal if we skipped some characters
  let emitLiteral() = seq {
    if acc <> [] then 
      yield acc |> List.rev |> toString |> Literal }

  // try parsing inline
  match parseInline chars, chars with
  | Some(body, chars), _ ->
      yield! emitLiteral ()
      // TODO: Produce single 'InlineCode' element
      // TODO: Continue recursively
  | _, c::chars ->
      // TODO: Add 'c' to unconsumed characters
      // and continue recursively
  | _, [] ->
      // Finished, emit the remaining unprocessed characters
      yield! emitLiteral () }

// ----------------------------------------------------------------------------

[<TestFixture>]
module Tests = 

  // parseInlineBody

  [<Test>]
  let ``End of inline is found`` () =
    let res = "aa`bb" |> List.ofSeq |> parseInlineBody []
    Assert.That((res = (['a'; 'a'], ['b'; 'b'])))

  [<Test>]
  let ``All input is consumed`` () =
    let res = "aa" |> List.ofSeq |> parseInlineBody []
    Assert.That((res = (['a'; 'a'], [])))

  // parseInline

  [<Test>]
  let ``Needs backtick`` () =
    let res = "aa" |> List.ofSeq |> parseInline
    Assert.That((res = None))

  [<Test>]
  let ``Finds inline code`` () =
    let res = "`aa`bb" |> List.ofSeq |> parseInline
    Assert.That((res = Some (['a'; 'a'], ['b'; 'b'])))

  // parseSpans

  [<Test>]
  let ``Parse two inline snippets`` () =
    let res = "`a` `c`" |> List.ofSeq |> parseSpans [] |> List.ofSeq
    Assert.That((res = [InlineCode "a"; Literal " "; InlineCode "c"]))

// ----------------------------------------------------------------------------

  do 
    ``End of inline is found``()
    ``All input is consumed``()
    ``Needs backtick`` ()
    ``Finds inline code`` ()
    ``Parse two inline snippets`` ()