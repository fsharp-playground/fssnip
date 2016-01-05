type exp = Num of int
         | Add of exp * exp

let rec eval e =
    match e with
    | Num e' -> e'
    | Add (e1,e2) -> eval e1 + eval e2