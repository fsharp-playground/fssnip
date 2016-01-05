type Expr =  Const of int
            | Add of (Expr * Expr)
            | Exit of Expr

let rec eval (e:Expr) (f:int -> int) = 
    match e with 
    | Const x -> f x
    | Add (a,b) -> eval b (fun z -> z + eval a f)
    | Exit x -> eval x (fun z -> z)
            
let a = eval (Add (Exit (Add (Exit (Const 1), Const 10)), Exit( Add( Const 8, Const 10) ))) (fun a -> a)