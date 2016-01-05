let fizzbuzz n = 
    let divisible x = (n % x) = 0
    match divisible 3,divisible 5 with
        | true,false -> "fizz"
        | false,true -> "buzz"
        | true,true -> "fizzbuzz"
        | false,false -> sprintf "%d" n

[1..15] |> List.map fizzbuzz
