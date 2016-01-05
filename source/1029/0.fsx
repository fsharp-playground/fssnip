open FsCheck
open Prop
 
let rec slowFib x = if x < 2 then 1 else slowFib(x - 2) + slowFib(x - 1)
 
let fastFib x =
    let rec fastFib' x n2 n1 =
        match x with
        |0 -> n2
        |_ -> fastFib' (x - 1) n1 (n2 + n1)
 
    if x < 2 then 1 else fastFib' x 1 1
 
[<Property>]
let ``Fast Fib matches the simple slow one``(x: int) =
    (x >= 0 && x < 25) ==> lazy (slowFib x = fastFib x)
    |> trivial (x < 2)
 
// Ok, passed 100 tests (30% trivial).