type 'e lam = ABS of string * 'e | APP of 'e * 'e | VAR of string

let lookup s = List.tryFind (fun (s', _) -> s = s')

let gensym = 
   let i = ref 0
   fun () -> 
        let result = sprintf "_%i" !i
        incr i
        result

let lamEval from ``to`` eval e = function
| ABS (s, b) -> 
    let s' = gensym ()
    ``to`` (ABS (s', eval ((s, ``to`` (VAR s'))::e) b))
| APP (f, x) -> 
    let f = eval e f
    let x = eval e x
    match from f with
    | Some (ABS (s, b)) -> 
        eval ((s, x)::e) b
    | _ ->
        ``to`` (APP (f, x))
| VAR s -> 
    match lookup s e with
    | Some (_, v) -> v
    | _ -> ``to`` (VAR s)

let lamToString toString = function
| ABS (s, b) -> sprintf @"(\%s.%s)" s (toString b)
| APP (f, x) -> sprintf "(%s %s)" (toString f) (toString x)
| VAR s      -> s

type 'e num = NUM of int | ADD of 'e * 'e | MUL of 'e * 'e

let numEval from ``to`` eval env = 
   let evalBop bop con (l, r) = 
      let l = eval env l
      let r = eval env r
      match (from l, from r) with
      | (Some (NUM l), Some (NUM r)) -> ``to`` (NUM (bop l r))
      | _                            -> ``to`` (con (l, r))
   function
   | NUM i -> ``to`` (NUM i)
   | ADD (x,y) -> evalBop (+) ADD (x,y)
   | MUL (x,y) -> evalBop (*) MUL (x,y)

let numToString toString = function
| NUM i -> string i
| ADD (l, r) -> sprintf "(%s + %s)" (toString l) (toString r)
| MUL (l, r) -> sprintf "(%s * %s)" (toString l) (toString r)

type 'e cons = CONS of 'e * 'e | FST of 'e | SND of 'e

let consToString toString = function
| CONS (l, r) -> sprintf "(%s, %s)" (toString l) (toString r)
| FST e       -> sprintf "(#1 %s)" (toString e)
| SND e       -> sprintf "(#2 %s)" (toString e)

let consEval from ``to`` eval e = 
   let prj f c t = 
      let t = eval e t
      match from t with
      | Some (CONS(x,y)) -> f(x,y)
      | _             -> ``to`` (c t)
   function
   | CONS (f, s) -> ``to`` (CONS (eval e f, eval e s))
   | FST t       -> prj fst FST t
   | SND t       -> prj snd SND t


type t = L of t lam | N of t num | C of t cons

let rec toString = function
| L l -> lamToString  toString l
| N n -> numToString  toString n
| C c -> consToString toString c

let rec eval e = function
| L l -> lamEval  (function | L l -> Some l | _ -> None) L eval e l
| N n -> numEval  (function | N n -> Some n | _ -> None) N eval e n
| C c -> consEval (function | C c -> Some c | _ -> None) C eval e c

let e = L (APP (L (ABS ("x",
                        N (ADD (C (FST (L (VAR "x"))),
                                N (NUM 1))))),
                C (CONS (N (NUM 2), N (NUM 3)))))

let (N (NUM 3)) = eval [] e
let "((\\x.((#1 x) + 1)) (2, 3))" = toString e
let (L (VAR "y")) = eval [] (L (APP (L (ABS ("x", L (VAR "x"))), L (VAR "y"))))
