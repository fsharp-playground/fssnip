let fizzRules = 
    [
        (fun i s -> if i % 3 = 0 then s + "Fizz" else s)
        (fun i s -> if i % 5 = 0 then s + "Buzz" else s)
        (fun i s -> if i % 7 = 0 then s + "Bazz" else s)
        (fun i s -> if i % 11 = 0 then s + "Bop" else s)
        (fun i s -> if s = "" then i.ToString() else s)
    ]

let executeRules rules accum i =
    let allRules =
        rules
            |> Seq.map (fun f -> f i)
            |> Seq.reduce ((fun x -> x) (>>))
    allRules accum

let fizzBuzz = executeRules fizzRules ""

[ 1 .. 105 ]
    |> Seq.map fizzBuzz
    |> Seq.iter (printfn "%s")