type Prim =
    | Add
    | Sub
    | Mul
    | Div
    | Eq
    | Not

type Value =
    | Bool of bool
    | Int of int
    | Lambda of (list<Expr> -> Expr)

and Expr =
    | Apply of Expr * list<Expr>
    | Call of Prim * list<Expr>
    | Const of Value
    | If of Expr * Expr * Expr
    | Let of Expr * (Expr -> Expr)
    | LetRec of (Lazy<Expr> -> Expr * Expr)

let Op prim =
    match prim with
    | Add ->
        fun [Int x; Int y] -> Int (x + y)
    | Sub ->
        fun [Int x; Int y] -> Int (x - y)
    | Mul ->
        fun [Int x; Int y] -> Int (x * y)
    | Div ->
        fun [Int x; Int y] -> Int (x / y)
    | Eq  ->
        function
        | [Int x; Int y] -> Bool (x = y)
        | [Bool x; Bool y] -> Bool (x = y)
    | Not -> fun [Bool x] ->
        Bool (not x)

let (|Binary|_|) (expr: Expr) =
    match expr with
    | Call (p, [x; y]) -> Some (p, x, y)
    | _                -> None

let rec Eval (expr: Expr) : Value =
    match expr with
    | Apply (f, xs) ->
        match Eval f with
        | Lambda f ->
            Eval (f xs)
    | Call (p, xs) ->
        Op p (List.map Eval xs)
    | Const x ->
        x
    | If (x, y, z) ->
        match Eval x with
        | Bool true  -> Eval y
        | Bool false -> Eval z
    | Let (x, f) ->
        Eval (f (Const (Eval x)))
    | LetRec f ->
        let rec x = lazy fst pair
        and body  = snd pair
        and pair  = f x
        Eval body

let rec Fac x =
    if x = 0 then 1 else x * Fac (x - 1)

let Fac10 =
    let i x = Const (Int x)
    let ( =? ) a b = Call (Eq, [a; b])
    let ( *? ) a b = Call (Mul, [a; b])
    let ( -? ) a b = Call (Sub, [a; b])
    let ( ^^ ) f x = Apply (f, [x])
    LetRec <| fun fac ->
        let fac =
            fun [x] ->
                let (Lazy fac) = fac
                If (x =? i 0, i 1, x *? (fac ^^ (x -? i 1)))
            |> Lambda
            |> Const
        (fac, fac ^^ i 10)

Fac 10
|> printfn "%A"

Eval Fac10
|> printfn "%A"
