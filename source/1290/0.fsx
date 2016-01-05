let fizzbuzz n =
    match n%3, n%5 with
    | 0,0 -> "FizzBuzz"
    | 0,_ -> "Fizz"
    | _,0 -> "Buzz"
    | _   -> string n

let genfizzbuzz n = seq { for x in 1..n -> fizzbuzz x }

let testFizzBuzz n =
    genfizzbuzz n
    |> Seq.skip (n-1)
    |> Seq.head