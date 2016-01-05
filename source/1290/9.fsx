//[snippet: Generate a FizzBuzz result set given two predicates and a max value]
let getFizzBuzz pred1 pred2 (n: int) =
    match pred1 n, pred2 n with
    | true,true -> "FizzBuzz"
    | true,_    -> "Fizz"
    | _,true    -> "Buzz"
    | _         -> string n

let fizzbuzz pred1 pred2 =
    let getFizzBuzz = getFizzBuzz pred1 pred2
    Seq.unfold(fun n -> Some(getFizzBuzz n, n+1)) 1
//[/snippet]



//[snippet: Generate the Coding Horror FizzBuzz]
fizzbuzz (fun x -> x%3=0) (fun x -> x%5=0) |> Seq.take 100 |> Seq.toArray
(* Results:
val it : string [] =
  [|"1"; "2"; "Fizz"; "4"; "Buzz"; "Fizz"; "7"; "8"; "Fizz"; "Buzz"; "11";
    "Fizz"; "13"; "14"; "FizzBuzz"; "16"; "17"; "Fizz"; "19"; "Buzz"; "Fizz";
    "22"; "23"; "Fizz"; "Buzz"; "26"; "Fizz"; "28"; "29"; "FizzBuzz"; "31";
    "32"; "Fizz"; "34"; "Buzz"; "Fizz"; "37"; "38"; "Fizz"; "Buzz"; "41";
    "Fizz"; "43"; "44"; "FizzBuzz"; "46"; "47"; "Fizz"; "49"; "Buzz"; "Fizz";
    "52"; "53"; "Fizz"; "Buzz"; "56"; "Fizz"; "58"; "59"; "FizzBuzz"; "61";
    "62"; "Fizz"; "64"; "Buzz"; "Fizz"; "67"; "68"; "Fizz"; "Buzz"; "71";
    "Fizz"; "73"; "74"; "FizzBuzz"; "76"; "77"; "Fizz"; "79"; "Buzz"; "Fizz";
    "82"; "83"; "Fizz"; "Buzz"; "86"; "Fizz"; "88"; "89"; "FizzBuzz"; "91";
    "92"; "Fizz"; "94"; "Buzz"; "Fizz"; "97"; "98"; "Fizz"; "Buzz"|]
*)
//[/snippet]

//[snippet: FizzBuzz tester]
// NOTE: Since you pass in the predicates, you will always receive a result of 'true'.
let testFizzBuzz n =
    let pred1, pred2 = (fun x -> x%3=0), (fun x -> x%5=0)
    getFizzBuzz pred1 pred2 n
        = (fizzbuzz pred1 pred2
           |> Seq.skip (n-1)
           |> Seq.head)
//[/snippet]