open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq.QuotationEvaluation
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns


type exp = 
 Int of int | Varr of string | App of string * exp
 | Add of exp * exp | Sub of exp * exp
 | Mul of exp * exp | Div of exp * exp | Ifz of exp * exp * exp

type def = Declaration of string * string * exp

type prog = Program of def list * exp

// a program to calculate the factorial of 10
let y = Program ([Declaration
                    ("fact","x", 
                     Ifz(Varr "x", Int 1,
                         Mul(Varr"x", (App ("fact", Sub(Varr "x",Int 1))))))
                 ],
                 App ("fact", Int 10))


let rec eval2 e env (fenv : (string * Expr<int -> int>) list) =
    match e with
    | Int i -> <@ i @>
    | Varr s -> env |> List.find (fun (a, b) -> a = s) |> snd
    | App (s,e2) -> let f = fenv |> List.find (fun (a, b) -> a = s) |> snd
                    let arg = (eval2 e2 env fenv)
                    <@ (%f)(%arg) @>
    | Add (e1,e2)    -> <@ %(eval2 e1 env fenv) + %(eval2 e2 env fenv) @>
    | Sub (e1,e2)    -> <@ %(eval2 e1 env fenv) - %(eval2 e2 env fenv) @>
    | Mul (e1,e2)    -> <@ %(eval2 e1 env fenv) * %(eval2 e2 env fenv) @>
    | Div (e1,e2)    -> <@ %(eval2 e1 env fenv) / %(eval2 e2 env fenv) @>
    | Ifz (e1,e2,e3) -> <@ if %(eval2 e1 env fenv) = 0 then %(eval2 e2 env fenv) else %(eval2 e3 env fenv) @>

let rec peval2 p env fenv : Expr<int>=
    match p with
    | Program ([],e) -> eval2 e env fenv
    | Program (Declaration (s1,s2,e1)::tl,e) ->
        let fDummyVar = Var("fDummy", typeof<int -> int>)
        let xDummyVar = Var("xDummy", typeof<int>)
        let fDummy = Expr.Var(fDummyVar)
        let xDummy = Expr.Var(xDummyVar)
        let r = <@ let rec f (x : int) = 
                        %(eval2 e1 ((s2,<@ %%xDummy @>)::env) ((s1, <@ %%fDummy @>)::fenv))
                        in %(peval2 (Program(tl,e)) env ((s1,<@  %%fDummy @>)::fenv)) 
                @>
        let fActual, xActual = 
            match r with
            | LetRecursive([f,Lambda(x, _)],_) -> f, x
            | _ -> failwith "unexpected"
        r.Substitute(
            fun v -> match v with
                        | a  when a = fDummyVar -> Expr.Var(fActual) |> Some
                        | b  when b = xDummyVar -> Expr.Var(xActual) |> Some
                        | _                     -> Expr.Var(v) |> Some) |> Expr.Cast

let z = peval2 y [] []

let w = Swensen.Unquote.Operators.eval z // result should be 3628800
