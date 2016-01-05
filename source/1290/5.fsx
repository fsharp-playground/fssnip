//[snippet: Generate a FizzBuzz result set given two predicates and a max value]
let getFizzBuzz pred1 pred2 (n: int) =
    match pred1 n, pred2 n with
    | 0,0 -> "FizzBuzz"
    | 0,_ -> "Fizz"
    | _,0 -> "Buzz"
    | _   -> string n

let fizzbuzz pred1 pred2 n =
    seq { for x in 1..n -> getFizzBuzz pred1 pred2 x }
//[/snippet]

//[snippet: Generate the Coding Horror FizzBuzz]
fizzbuzz (fun x -> x%3) (fun x -> x%5) 100 |> Seq.toArray
//[/snippet]

//[snippet: FizzBuzz tester]
let testFizzBuzz n =
    let pred1, pred2 = (fun x -> x%3), (fun x -> x%5)
    getFizzBuzz pred1 pred2 n
        = (fizzbuzz pred1 pred2 n
           |> Seq.skip (n-1)
           |> Seq.head)
//[/snippet]