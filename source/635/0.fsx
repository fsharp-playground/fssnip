(* fibonacci numbers in O(n) with tail recursion *)
let fib n =
    let rec fib' a b i =
        if i=n then a+b
        else fib' b (a+b) (i+1)
    if n<=0 then 0
    elif n=1 || n=2 then 1
    else fib' 1 1 3


(** map with fold **)

let map' f lst = List.fold (fun acc x -> (f x)::acc) [] lst 
                 |> List.rev

map' (fun x -> x*2) [1;2;3;4;5]

(** convert a number to the decimal number system **)

let convert x radix = 
    List.foldBack2 (fun dig pow acc ->
                        acc + dig * (pown radix pow))
                   x
                   (List.rev [0..x.Length-1])
                   0

let convert' x radix = 
    List.mapi (fun i x -> x * (pown radix i)) (List.rev x)
    |> List.sum


let convert'' x radix = (* without library list functions *)
    match x with
    | [] -> 0
    | h::t  -> h * (pown radix (x.Length-1)) + 
                (convert t radix)

convert [1;1;1] 2 
(* 7 *)
convert [1;15] 16 
(* 31 *)

