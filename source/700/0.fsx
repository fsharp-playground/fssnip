(*[omit:(Parser Monad omitted. Code available here: https://bitbucket.org/ZachBray/parsad)]*)

open System.Text.RegularExpressions
open System

type text = string
type error = string

[<AutoOpen>]
module String =
   let isEmpty(str:string) =
      str.Trim().Length = 0

   let (|Empty|_|) str =
      if isEmpty str then Some()
      else None

type 'a Parser = 
   | Parser of (text -> error ref -> ('a * text) option)
   member x.Evaluate(text, error) =
      let (Parser f) = x
      f text error
   member x.Parse text =
      let error = ref ""
      match x.Evaluate(text, error) with
      | Some (y, Empty) -> y
      | Some _ | None ->
         failwith !error

type ParserBuilder() =
   let parse patterns text =
      let pattern = 
         patterns |> Seq.map (sprintf "(%s)")
         |> String.concat ""
      let regex = Regex (sprintf "^\s*%s" pattern)
      let matchAttempt = regex.Match text
      let groups =
         [ for group in matchAttempt.Groups -> group.Value ]
      match groups with
      | [] -> []
      | x::xs -> xs

   let parsePattern pattern (f:string -> 'a Parser) text error =
      match text |> parse [pattern; ".*"]  with
      | [value; rest] ->
         let g = f value
         g.Evaluate(rest, error)
      | _ -> 
         error := sprintf "Expected '%s' but found '%s'" pattern text
         None
         
   let parseInfix (left:unit -> 'a Parser) op (right:unit -> 'b Parser) (f:('a*string*'b) -> 'c Parser) text error =
      match text |> parse [".*"; op; ".*"] with
      | [x; op; y] ->
         match left().Evaluate(x, error) with
         | Some(x, Empty) ->
            match right().Evaluate(y, error) with
            | Some(y, rest) -> 
               f(x, op, y).Evaluate(rest, error)
            | None -> 
               error := sprintf "Expected expression but found '%s'" y
               None
         | Some _ | None ->
            error := sprintf "Expected expression but found '%s'" x
            None
      | _ ->
         error := sprintf "Expected '<x> %s <y>' but found '%s'" op text
         None

   let parseAny (parsers:(unit -> 'a Parser) list) (f: 'a -> 'b Parser) text error =
      parsers |> Seq.tryPick (fun parser ->
         match parser().Evaluate(text, error) with
         | Some(x, rest) ->
            let g = f x
            g.Evaluate(rest, error)
         | None -> None
      )

   member b.Bind (parsers, f) =
      Parser(parseAny parsers f)
   member b.Bind ((left, op, right), f) =
      Parser(parseInfix left op right f)
   member b.Bind (parser, f) = 
      b.Bind([parser], f)
   member b.Bind (pattern:string, f) = 
      Parser(parsePattern pattern f)
   member b.Return x =
      Parser(fun text error -> Some(x, text))
   member b.ReturnFrom(parsers:_ list) =
      b.Bind(parsers, b.Return)

let parser = ParserBuilder()

(*[/omit]*)

let number() = parser {
   let! n = "[0-9]+"
   return Int32.Parse n
}

let (<+>) x y () = parser {
   // Left associativity is by default for infix operators
   let! x, _, y = x, "\+", y
   return x + y
}

let (<->) x y () = parser {
   let! x, _, y = x, "\-", y
   return x - y
}

let (<*>) x y () = parser {
   let! x, _, y = x, "\*", y
   return x * y
}

let (</>) x y () = parser {
   let! x, _, y = x, "/",y
   return x / y
}

let bracketed (f:unit -> 'a Parser) () = parser{
   let! _ = "\("
   let! x = f
   let! _ = "\)"
   return x
}

let rec expression() = 
   parser {
      // Tries possible parsing steps in order
      // I.e., in reverse order of precedence
      return! [
         expression <+> expression
         expression <-> expression
         expression <*> expression
         expression </> expression
         bracketed expression
         number
      ]
   }

// Example
printfn "%A" (expression().Parse "(10 + (2 * 10 * 2))/5 - 14")