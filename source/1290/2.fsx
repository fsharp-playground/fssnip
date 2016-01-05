let fizzbuzz pred1 pred2 (n: int) =
    match pred1 n, pred2 n with
    | 0,0 -> "FizzBuzz"
    | 0,_ -> "Fizz"
    | _,0 -> "Buzz"
    | _   -> string n

let genFizzBuzz pred1 pred2 n =
    seq { for x in 1..n -> fizzbuzz pred1 pred2 x }

let testFizzBuzz n =
    genFizzBuzz ((%)3) ((%)5) n
    |> Seq.skip (n-1)
    |> Seq.head