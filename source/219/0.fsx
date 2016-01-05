open System

//Assert the convergence of a series.
let assertSeriesConvergence(expected : float) (delta : float) low high series =
    if low > high then 
        failwith "Incorrect boundaries for the series"
    else
        seq {for n = low to high do yield n}
        |> Seq.map (fun n -> Math.Abs((series n) - expected) <= delta) //build the assertions
        |> Seq.forall ((=) true)

//Naive factorial with tail recursion.
let (!) n  = let rec _fact n result = if n = 1.0 || n = 0.0 then result else _fact (n - 1.0) (result * n)
             _fact n 1.0

//Ramanujan Formula.
let Ramanujan x = 
    let sum = seq {for i = 0.0 to x do yield i}
               |> Seq.map (fun n -> (!(4.0 * n) * (1103.0 + (26390.0 * n))) / (((!n) ** 4.0) * (3964.0 ** (4.0 * n))))
               |> Seq.sum
    ((2.0 * (sqrt 2.0)) / 9801.0) * sum //any ideas?

assertSeriesConvergence (1.0 / Math.PI) (10.0 ** (-8.0)) 1.0 40.0 Ramanujan