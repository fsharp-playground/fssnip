open System
let (|Fizz|None|) x =
  if x % 3 = 0 then Fizz else None

let (|Buzz|None|) x =
  if x % 5 = 0 then Buzz else None

let fizzbuzz x =
  match x with
    | Fizz & Buzz -> "FizzBuzz"
    | Fizz -> "Fizz"
    | Buzz -> "Buzz"
    | _ -> x.ToString()

let start xs =
  for i in xs do
    printfn "%s" (fizzbuzz i)
