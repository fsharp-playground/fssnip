let fizzbuzz n = 
    let divisibleBy m = (n % m) = 0
    match divisibleBy 3,divisibleBy 5 with
        | true,false -> "fizz"
        | false,true -> "buzz"
        | true,true -> "fizzbuzz"
        | false,false -> sprintf "%d" n

[1..15] |> List.map fizzbuzz 