module LanguageCombinator

open FParsec
open System

exception Error of string

type Op = 
    | Plus
    | Minus
    | GreaterThan
    | LessThan
    | Mult
    | Divide
    | Carrot
       
type Ast =     
    | Statement of Ast    
    | Expression of Ex    
    | Function of string option * Argument list option * Ast
    | Scope of Ast list option
    | Class of Ex * Ast
    | Conditional of Ex * Ast * Ast option 
    | WhileLoop of Ex * Ast
    | ForLoop of Ex * Ex * Ex * Ast    
    | Call of string * Argument list option
    | Assign of Ex * Ex
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

let semicolon = skipStringCI ";"

let quote = skipStringCI "\""

let literal = quote >>. manyCharsTill anyChar quote |>> Literal

let floatNum = pfloat |>> Float

let intNum = pint32 |>> Int

(* Variables *)

let variable = many1Chars (satisfy isAsciiLetter) |>> Variable

(* Operators *)

let plus = pstring "+" >>% Plus

let minus = pstring "-" >>% Minus

let divide = pstring "/" >>% Divide

let mult = pstring "*" >>% Mult

let carrot = pstring "^" >>% Carrot

let gt = pstring ">" >>% GreaterThan

let lt = pstring  "<" >>% LessThan

let op = spaces >>. choice[plus;minus;divide;mult;carrot;gt;lt]

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

(* Scope *)

let funcInners, funcInnersImpl = createParserForwardedToRef()

let scope = parse{
    do! spaces
    do! skipStringCI "{"
    do! spaces
    let! text = opt funcInners
    do! spaces
    do! skipStringCI "}"    
    do! spaces
    return Scope(text)
}

(* Classes *)

let classItem = parse{
    do! skipStringCI "class"
    do! spaces
    let! name = variable    
    let! classStatements = scope
    do! spaces
    return Class(name, classStatements)
}    

(* Functions *)

let innerArgs = sepEndBy1 (expr |>> Element) (pstring ",")
let arguments = innerArgs |> between "(" ")"

let func = parse {
    do! skipStringCI "func"
    do! spaces
    let! name = opt (many1Chars (satisfy isAsciiLetter))
    let! arguments = opt arguments
    do! spaces 
    do! skipStringCI "->"
    let! scope = scope
    return Function(name, arguments, scope)
}


(* Conditionals *)

let conditionalParser, conditionalParserImpl = createParserForwardedToRef()

let ifBlock = parse{
    do! skipStringCI "if"
    let! condition = expr |> between "(" ")"
    do! spaces
    let! onTrue = scope
    do! spaces
    do! optional (skipStringCI "else")
    do! spaces
    let! onFalse = opt (conditionalParser <|> scope)
    return (condition, onTrue, onFalse)
}

do conditionalParserImpl:= ifBlock |>> Conditional

let conditional = ifBlock |>> Conditional

(* Loops *)

let whileLoop = (pstring "while" >>. spaces) >>. (expr |> between "(" ")") >>= fun predicate ->
                scope >>= fun body ->
                preturn (WhileLoop(predicate, body))


let forLoop = 
    let startCondition = expr .>> pstring ";"
    let predicate = expr .>> pstring ";"
    let endCondition = expr 
    let forKeyword = pstring "for" .>> spaces

    let forItems = tuple3 startCondition predicate endCondition |> between "(" ")"

    forKeyword >>. forItems .>>. scope >>= fun ((s, p, e), b) ->
        preturn (s, p, e, b) |>> ForLoop
        

(* Function calls *)

let call = parse{
    let! name = many1Chars (satisfy isAsciiLetter)
    do! spaces
    do! skipStringCI "("
    let! args = opt innerArgs
    do! spaces
    do! skipStringCI ")"
    do! spaces
    return Call(name, args)
}           

(* Assignment *)

let assign = parse{
    let! ex = expr
    do! spaces
    do! skipStringCI "="
    do! spaces
    let! assignEx = expr
    do! spaces    
    return (ex, assignEx) |> Assign
}

(* Statements *)


let delineatedStatement = choice[
                                 attempt call;
                                 attempt assign;
                                 expr |>> Expression
                                ] .>> semicolon |>> Statement

let statement = choice[
                        conditional;
                        whileLoop;
                        forLoop;                  
                        delineatedStatement
                     ]
                                           

(* things that can be in functions *)

do funcInnersImpl := many1 (spaces >>? choice [scope; func; statement])

(* Things can be in the program root *)

let programLines = spaces >>. choice[
                                        classItem;
                                        func;
                                        scope;
                                        statement
                                    ]

(* The full program *)
                                    
let program = many programLines

let test input = match run (program .>> eof) input with
                    | Success(r,_,_) -> r
                    | Failure(r,_,_) -> 
                            Console.WriteLine r
                            raise (Error(r))