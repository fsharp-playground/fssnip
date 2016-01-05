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

(* Usage *)
let concat = [1;2;3] ++ [4;5]
>> [1;2;3;4;5]
let application = (fun x -> x + 6) $ 6
>> 12
let r = 3 ^ 5
>> 243
let r2 = 3 ^^ 5
>> 243
let x = 10
let r3 = lazy (x + 10)
let final = ! r3
>> 20
