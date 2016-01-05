module LanguageCombinator

open FParsec
open System

exception Error of string

type Op = 
    | Plus
    | Minus
        
type Ast =     
    | Statement of Ast
    | List of Ast list  
    | Expression of Ex    
    | Function of Argument list option * Ast option
and Ex =
    | Single of Ast
    | Full of Ex * Op * Ex
    | Float of float
    | Int of int
    | Literal of string
    | Variable of string
and Argument = 
    | Element of Ex

(* Literals *)

let quote = pstring "\""

let literal = quote >>. manyCharsTill anyChar quote |>> Literal

let floatNum = pfloat |>> Float

let intNum = pint32 |>> Int

(* Variables *)

let variable = many1Chars (satisfy isAsciiLetter) |>> Variable

(* Operators *)

let plus = pstring "+" >>% Plus

let minus = pstring "-" >>% Minus

let op = spaces >>. choice[plus;minus]

(* expressions *)

// create a forward reference 
// the expr is what we'll use in our parser combinators
// the exprImpl we'll populate iwth all the recursive options later
let expr, exprImpl = createParserForwardedToRef()

let expression1 = spaces >>? choice[floatNum;intNum;literal;variable] 

let between a b p = pstring a >>. p .>> pstring b

let bracketExpression = expr |> between "(" ")"

let lhExpression = choice[expression1; bracketExpression]

let expressionOpration =  lhExpression                           >>=? fun operandL ->
                          op                                     >>=? fun operator ->
                          choice[expr;bracketExpression]         >>=? fun operandR ->
                          preturn (operandL, operator, operandR) |>> Full 

do exprImpl := spaces >>. choice[attempt expressionOpration <|> attempt bracketExpression <|> expression1 ]

(* Statements *)

let statement = expr .>> pstring ";"

let statements = many1 (spaces >>? statement|>> Expression) |>> List

(* Functions *)

let arguments = sepEndBy1 (expr |>> Element) (pstring ",") |> between "(" ")"

let func = parse {
    do! skipStringCI "func"
    do! spaces
    let! arguments = opt arguments 
    do! skipStringCI "->"
    do! spaces
    do! skipStringCI "{"
    do! spaces
    let! text = opt statements
    do! spaces
    do! skipStringCI "}"    
    do! spaces
    return Function(arguments, text)
}

(* Program lines *)

let programLines = spaces >>. choice[
                                        func;
                                        statements
                                    ]

let program = many programLines

let test input = match run (program .>> eof) input with
                    | Success(r,_,_) -> r
                    | Failure(r,_,_) -> 
                            Console.WriteLine r
                            raise (Error(r))