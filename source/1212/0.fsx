module fs
    
open NUnit.Framework
open FsUnit
open System
open System.Data
    
type Operation = 
    | Mult 
    | Sub
    | Add
    override this.ToString() = 
        match this with  
            | Mult -> "*" 
            | Sub -> "-" 
            | Add -> "+"
    member this.evaluate = 
        match this with  
            | Mult -> (*) 
            | Sub -> (-) 
            | Add -> (+)
    
let orderofOps = [[Mult];[Add;Sub]]

type Expression = 
    | Terminal of int    
    | Expr of Expression * Operation * Expression
    
let rand = new System.Random()
    
let randNum min max = rand.Next(min, max)
    
let randomOperation () = 
    match randNum 0 2 with 
    | 0 -> Mult 
    | 1 -> Sub  
    | _ -> Add

let (|TermWithExpression|_|) predicate expr  = 
       match expr with 
        | Expr(Terminal(left), targetOp, Expr(Terminal(right), o, next)) 
            when predicate targetOp -> Expr(Terminal(targetOp.evaluate left right), o, next) |> Some
        | _ -> None

let (|TermWithTerm|_|) predicate expr = 
    match expr with 
        | Expr(Terminal(item), targetOp, Terminal(item2)) 
                when predicate targetOp -> 
                    Terminal(targetOp.evaluate item item2) |> Some
        | _ -> None

let foldExpr expr opsInPrecedence = 
    let rec foldExpr' expr = 
        let shouldEvalOperator o = List.exists (fun i -> i = o) opsInPrecedence

        match expr with 
            | TermWithExpression shouldEvalOperator output -> foldExpr' output    
            | TermWithTerm shouldEvalOperator output -> output
            | Expr(left, o, right) -> Expr(foldExpr' left, o, foldExpr' right)            
            | Terminal(i) -> Terminal(i)

    foldExpr' expr
    
let rec randomExpression min max length = 
    match length with 
        | 0 -> Terminal(randNum min max)
        | _ -> Expr(Terminal(randNum min max), randomOperation(), randomExpression min max (length - 1))
    
let rec display = function
        | Terminal(i) -> i.ToString()
        | Expr(i, op, exp) -> System.String.Format("{0} {1} {2}",display i, op,display exp)        
    
let eval e = List.fold foldExpr e orderofOps
    
[<Test>]
let arithmeticTest() =
 
    let dt = new DataTable()    

    for i in [0..100] do
        let randomExpr = randomExpression 0 10 5

        let validationResult = dt.Compute(display randomExpr, "").ToString() |> Convert.ToInt32
    
        let result = eval randomExpr
            
        printfn "%s = %d = %d" (display randomExpr) validationResult (match result with Terminal(x) -> x)
    
        result |> should equal <| Terminal(validationResult)