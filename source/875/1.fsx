let fizzRules = 
    [
        (fun i -> if i % 3 = 0 then "Fizz" else "")
        (fun i -> if i % 5 = 0 then "Buzz" else "")
        (fun i -> if i % 7 = 0 then "Bazz" else "")
        (fun i -> if i % 11 = 0 then "Bop" else "")
    ]

let fizzbuzz rules i =
    let rec ruleRunner s rl =
        match s, rl with
            | "", []
                ->  i.ToString()
            | _, []
                ->   s
            | _, h::t
                ->  ruleRunner (s + h i) t
    ruleRunner "" rules

[ 1 .. 105 ]
    |> Seq.map (fizzbuzz fizzRules)
    |> Seq.iter (printfn "%s")