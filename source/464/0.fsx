let multiplesOf x = Seq.initInfinite (fun i -> (i+1) * x)

let leastCommonDenominator x y =
    Seq.find (fun i -> i%x = 0) (multiplesOf y)

let problem5 n =
    let testDenominator lcd =
        List.forall (fun i -> lcd % i = 0) [n-2 .. -1 .. 1]

    let rec problem5Helper (x, lcd) =
        match testDenominator lcd with
            | true -> lcd
            | false ->
                let lcd2 = leastCommonDenominator (x-1) (x-2)
                problem5Helper (x-1, (leastCommonDenominator lcd lcd2))
    
    problem5Helper (n, (leastCommonDenominator n n-1))