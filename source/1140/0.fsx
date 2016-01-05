type Number = Zero | Integer of int | Float of float;;
let add n1 n2 =
    match n1, n2 with
    | Zero, n | n, Zero -> n
    | Integer i1, Integer i2 -> Integer (i1+i2)
    | Float f1, Float f2 -> Float (f1+f2)
    | Float f1, Integer i2 -> Float (f1 + (float i2))
    | Integer i1, Float f2 -> Float ((float i1) + f2);;

let i = 1;;
let f = 2.0;;

add i f;;

(* In OCaml: yields 3.0
   In F#: error -> "error FS0001: This expression was expected to have type
    Number    
but here has type
    int" 
*)