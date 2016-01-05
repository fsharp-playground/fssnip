(* The start of a Haskell Operator Module *)
(* Application opeator *)
let ($) a b = a b
(* List concat operator *)
let (++) a b = List.append a b
(* Raise-to-the-power operators *)
let (^) (a: int) (b: int) = [1..b] |> List.map(fun _ -> a) |> List.reduce((*))
let (^^) (a: int) (b: int) = [1..b] |> List.map(fun _ -> a) |> List.reduce((*))
(* Force evaluation (strictness flag) *) 
let (!) (a: Lazy<'a>) = a.Force()