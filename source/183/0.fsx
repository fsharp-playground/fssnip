module Parser = 
  
  (*
  F# implementation of a generic Top-Down-Operator-Precedence Parser 
  as described in this paper http://portal.acm.org/citation.cfm?id=512931.

  The parser has been extended to allow for statements in comparison to Pratt's
  original algorithm which only parsed languages which use expression-only grammar.

  The parsers is "impure" in the sense that is uses a ref-cell for storing the
  input in the T<_, _, _> record class, this is soley for performance reasons
  as it's to expensive to create a new record object for every consumed token.

  Certain functions also throw exceptions, which generally also is considered impure.

  More information:
  * http://en.wikipedia.org/wiki/Vaughan_Pratt (Original Inventor)
  * http://en.wikipedia.org/wiki/Pratt_parser (Alias name)
  * http://effbot.org/zone/simple-top-down-parsing.htm (Python implementation)
  * http://javascript.crockford.com/tdop/tdop.html (JavaScript implementation)
  *)

  type Pos = int * int

  type T<'a, 'b, 'c> when 'c : comparison = {
      Input : 'a list ref
      Null : Map<'c, 'a -> T<'a, 'b, 'c> -> 'b>
      Stmt : Map<'c, 'a -> T<'a, 'b, 'c> -> 'b>
      Left : Map<'c, 'a -> 'b -> T<'a, 'b, 'c> -> 'b>
      Bpwr : Map<'c, int>
      Type : 'a -> 'c
      Line : 'a -> Pos
  }
  
  type Pattern<'a, 'b, 'c> when 'c : comparison 
    = Sym of 'c
    | Get of (T<'a, 'b, 'c> -> 'b)

  type Exn (msg, pos) = 
    inherit System.Exception(msg)
    member x.Position = pos

  let exn msg = Exn(msg, (-1, -1)) |> raise
  let private exn_line pos msg = Exn(msg, pos) |> raise

  let private unexpected_end () = "Unexpected end of input" |> exn
  let private type_mismatch a b pos = 
    let msg = sprintf "Expected token of type %A but found %A" a b
    msg |> exn_line pos

  let inline private call_nud tok x = x.Null.[x.Type tok] tok x
  let inline private call_smd tok x = x.Stmt.[x.Type tok] tok x
  let inline private call_led tok left x = x.Left.[x.Type tok] tok left x
  let inline private get_bpw tok x = 
    match x.Bpwr.TryFind (x.Type tok) 
      with Some pwr -> pwr | _ -> 0

  let current_exn x =  
    match !x.Input with
    | x::_ -> x
    | _ -> unexpected_end ()

  let current x = 
    match !x.Input with
    | x::_ -> Some x
    | _ -> None

  let current_type x =
    match x |> current with
    | None -> None
    | Some tok -> Some(tok |> x.Type)

  let current_type_exn x = 
    x |> current_exn |> x.Type

  let skip x =
    match !x.Input with
    | _::xs -> x.Input := xs
    | _ -> unexpected_end ()

  let skip_if type' x =
    match !x.Input with
    | tok::xs when x.Type tok = type' -> x.Input := xs
    | tok::_ -> type_mismatch type' (x.Type tok) (x.Line tok)
    | _ -> unexpected_end ()

  let skip_if_try type' x =
    match !x.Input with
    | tok::xs when x.Type tok = type' -> x.Input := xs; true
    | tok::_ -> false
    | _ -> false
   
  let expr rbpw x =
    let rec expr left =
      match x |> current with
      | Some tok when rbpw < (x |> get_bpw tok) -> 
        x |> skip
        expr (x |> call_led tok left)

      | _ -> left

    let tok = x |> current_exn
    x |> skip
    expr (x |> call_nud tok)

  let expr_then then' x =
    let expr = x |> expr 0
    x |> skip_if then'
    expr

  let expr_any x = expr 0 x

  let rec expr_list x =
    match !x.Input with
    | [] -> []
    | _ -> (x |> expr 0) :: (x |> expr_list)

  let stmt term x =
    let tok = x |> current_exn
    match x.Stmt.TryFind (tok |> x.Type) with
    | None -> 
      let xpr = x |> expr 0
      x |> skip_if term
      xpr

    | Some f ->
      x |> skip
      f tok x

  let rec stmt_list term x =
    match !x.Input with
    | [] -> []
    | _ -> (x |> stmt term)  :: (x |> stmt_list term)

  let match' pat parser =

    let rec match' pat acc p =
      match pat with
      | [] -> acc |> List.rev

      | Sym(s)::pat -> 
        if p |> skip_if_try s
          then p |> match' pat acc
          else []

      | Get(f)::pat ->
        let acc = (f p :: acc)
        p |> match' pat acc

    parser |> match' pat []

  let match_error () = failwith ""

  let create<'a, 'b, 'c when 'c : comparison> type' line = {
    Input = ref []
    Null = Map.empty<'c, 'a -> T<'a, 'b, 'c> -> 'b>
    Stmt = Map.empty<'c, 'a -> T<'a, 'b, 'c> -> 'b>
    Left = Map.empty<'c, 'a -> 'b -> T<'a, 'b, 'c> -> 'b>
    Bpwr = Map.empty<'c, int>
    Type = type'
    Line = line
  }

  let smd token parser x = {x with T.Stmt = x.Stmt.Add(token, parser)}
  let nud token parser x = {x with T.Null = x.Null.Add(token, parser)}
  let led token parser x = {x with T.Left = x.Left.Add(token, parser)}
  let bpw token power  x = {x with T.Bpwr = x.Bpwr.Add(token, power)}
    
  let run_expr input parser =
    {parser with T.Input = ref input} |> expr_list

  let run_stmt term input parser =
    {parser with T.Input = ref input} |> stmt_list term

(*

Example parser for a very simple grammar, assumes the Parser module exists in current namespace

*)

//AST Types
type UnaryOp
  = Plus
  | Minus
  
type BinaryOp
  = Multiply
  | Add
  | Subtract
  | Divide
  | Assign
  | Equals

type Ast
  = Number of int
  | Identifier of string
  | String of string
  | Binary of BinaryOp * Ast * Ast
  | Unary of UnaryOp * Ast
  | Ternary of Ast * Ast * Ast // test * ifTrue * ifFalse
  | If of Ast * Ast * Ast option // test + ifTrue and possibly ifFalse (else branch)
  | Call of Ast * Ast list // target + arguments list
  | Block of Ast list // statements list
  | True
  | False

//Shorthand types for convenience
module P = Parser
type Token = string * string * (Parser.Pos)
type P = Parser.T<Token, Ast, string>

//Utility functions for extracting values out of Token
let type' ((t, _, _):Token) = t
let value ((_, v, _):Token) = v
let pos ((_, _, p):Token) = p
let value_num (t:Token) = t |> value |> int

//Utility functions for creating new tokens
let number value pos : Token = "#number", value, pos
let string value pos : Token = "#string", value, pos
let identifier name pos : Token = "#identifier", name, pos

let symbol type' pos : Token = type', "", pos
let add = symbol "+"
let sub = symbol "-"
let mul = symbol "*"
let div = symbol "/"
let assign = symbol "="
let equals = symbol "=="
let lparen = symbol "("
let rparen = symbol ")"
let lbrace = symbol "{"
let rbrace = symbol "}"
let comma = symbol ","
let qmark = symbol "?"
let colon = symbol ":"
let scolon = symbol ";"
let if' = symbol "if"
let true' = symbol "true"
let else' = symbol "else"

//Utility functions for converting tokens to binary and unary operators
let toBinaryOp tok =
  match type' tok with
  | "=" -> BinaryOp.Assign
  | "+" -> BinaryOp.Add
  | "-" -> BinaryOp.Subtract
  | "*" -> BinaryOp.Multiply
  | "/" -> BinaryOp.Divide
  | "==" -> BinaryOp.Equals
  | _ -> P.exn (sprintf "Couldn't convert %s-token to BinaryOp" (type' tok))

let toUnaryOp tok =
  match type' tok with
  | "+" -> UnaryOp.Plus
  | "-" -> UnaryOp.Minus
  | _ -> P.exn (sprintf "Couldn't convert %s-token to UnaryOp" (type' tok))

//Utility function for defining infix operators
let infix typ pwr p =
  let infix tok left (p:P) =
    Binary(tok |> toBinaryOp, left, p |> P.expr pwr)

  p |> P.bpw typ pwr |> P.led typ infix
  
//Utility function for defining prefix operators
let prefix typ p =
  let prefix tok (p:P) =
    Unary(tok |> toUnaryOp, p |> P.expr 70)

  p |> P.nud typ prefix
  
//Utility function for defining constants
let constant typ value p =
  p |> P.nud typ (fun _ _ -> value)

//Utility function for parsing a block 
let block p =
  let rec stmts p =
    match p |> P.current_type with
    | None -> []
    | Some "}" -> p |> P.skip; []
    | _ -> (p |> P.stmt ";") :: (stmts p)

  p |> P.skip_if "{"
  Block(p |> stmts)

//The parser definition
let example_parser =
  (P.create type' pos)

  //Literals and identifiers
  |> P.nud "#number" (fun t _ -> t |> value |> int |> Number) 
  |> P.nud "#identifier" (fun t _ -> t |> value |> Identifier)
  |> P.nud "#string" (fun t _ -> t |> value |> String)

  //Constants
  |> constant "true" Ast.True
  |> constant "false" Ast.False
  
  //Infix Operators <expr> <op> <expr>
  |> infix "==" 40
  |> infix "+" 50
  |> infix "-" 50
  |> infix "*" 60
  |> infix "/" 60
  |> infix "=" 80

  //Prefix Operators <op> <expr>
  |> prefix "+"
  |> prefix "-"
  
  //Grouping expressions (<expr>)
  |> P.nud "(" (fun t p -> p |> P.expr_then ")")

  //Ternary operator <expr> ? <expr> : <expr>
  |> P.bpw "?" 70
  |> P.led "?" (fun _ left p ->
      let ternary = [P.Get P.expr_any; P.Sym ":"; P.Get P.expr_any]
      match p |> P.match' ternary with
      | ifTrue::ifFalse::_ -> Ternary(left, ifTrue, ifFalse)
      | _ -> P.match_error()
    )

  //If/Else statement if(<condition>) { <exprs } [else { <exprs> }]
  |> P.smd "if" (fun _ p ->
      let if' = [P.Sym "("; P.Get P.expr_any; P.Sym ")"; P.Get block]
      let else' = [P.Sym "else"; P.Get block]

      match p |> P.match' if' with
      | test::ifTrue::_ -> 
        match p |> P.match' else' with
        | ifFalse::_ -> If(test, ifTrue, Some(ifFalse))
        | _ -> If(test, ifTrue, None)
      | _ -> P.match_error()
    )

  //Function call
  |> P.bpw "(" 80
  |> P.led "(" (fun _ func p ->
      let rec args (p:P) =
        match p |> P.current_type_exn with
        | ")" -> p |> P.skip; []
        | "," -> p |> P.skip; args p
        | _ -> (p |> P.expr_any) :: args p

      Call(func, args p)
    )
    
//Code to parse
(*
1: x = 5;
2: y = 5;
3: 
4: if(x == y) {
5:   print("x equals y");
6: } else {
7:   print("x doesn't equal y");
8: }
*)

//The code in tokens, manually entered
//since we don't have a lexer to produce
//the tokens for us
let tokens = 
  [
    //x = 5;
    identifier "x" (1, 1)
    assign (1, 3)
    number "5" (1, 5)
    scolon (1, 6)

    //y = 5;
    identifier "y" (2, 1)
    assign (2, 3)
    number "5" (2, 5)
    scolon (2, 6)

    //if(x == y) {
    if' (4, 1)
    lparen (4, 3)
    identifier "x" (4, 4)
    equals (4, 6)
    identifier "y" (4, 9)
    rparen (4, 10)
    lbrace (4, 12)

    //print("x equals y");
    identifier "print" (5, 3)
    lparen (5, 7)
    string "x equals y" (5, 9)
    rparen (5, 21)
    scolon (5, 22)

    //} else {
    rbrace (6, 1)
    else' (6, 3)
    lbrace (6, 7)

    //print("x doesn't equal y");
    identifier "print" (7, 3)
    lparen (7, 7)
    string "x doesn't equal y" (7, 9)
    rparen (7, 27)
    scolon (7, 28)

    //}
    rbrace (8, 1)
  ]

let ast =
  example_parser |> P.run_stmt ";" tokens
