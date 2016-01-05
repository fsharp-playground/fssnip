// [snippet: (an imitation) pipeline operator]
module NinjaOperators =
    let private print x y = printfn "%A ==> %A" x y

    let ( |> ) x f = let result = f x in print x result ; result
    let ( <| ) f x = x |> f

    let ( ||> ) t f = t |> fun (x, y) -> f x y
    let ( <|| ) f t = t ||> f

    let ( |||> ) t f = t |> fun (x, y, z) -> f x y z
    let ( <||| ) f t = t |||> f

// [/snippet]

// [snippet: Usage]
#if INTERACTIVE
// Please call the NinjaOperators module when you need to debug.
open NinjaOperators

// If you want to reset fsi, please open the Operators module again.
// > open Operators ;;
#endif

let f x = x * 10
let g x = x + 10

let sample1 () = 10 |> f |> g
// Output:
// 10 ==> 100
// 100 ==> 110
// val it : int = 110

let sample2 () =
    [1 .. 5]
    |> List.map f
    |> List.map g
    |> List.sum
// Output:
// [1; 2; 3; 4; 5] ==> [10; 20; 30; 40; 50]
// [10; 20; 30; 40; 50] ==> [20; 30; 40; 50; 60]
// [20; 30; 40; 50; 60] ==> 200
// val it : int = 200

let sample3 () = g <| (f <| 10)
// Output:
// 10 ==> 100
// 100 ==> 110
// val it : int = 110

let sample4 () = (1, 2) ||> ( + )
// Output:
// (1, 2) ==> 3
// val it : int = 3

let sample5 () = ( * ) <|| (10, 10)
// Output:
// (10, 10) ==> 100
// val it : int = 100

let sample6 () = (1, 2, 3) |||> fun x y z -> x + y + z
// Output:
// (1, 2, 3) ==> 6
// val it : int = 6

let sample7 () = (fun x y z -> x * y * z) <||| (1, 2, 3)
// Output:
// (1, 2, 3) ==> 6
// val it : int = 6

// [/snippet]