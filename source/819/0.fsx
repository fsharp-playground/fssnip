open System

// [snippet: Basic approach to Project Euler 34]
let Factorial n = 
    if n = 0 then 1
    else if n < 3 then n
    else
        let rec NumProduct (list: int list) total =
            if list.Length > 1 then 
                let num = list.Head
                let newTotal = total * num
                NumProduct list.Tail newTotal
            else total

        let numbers = [n.. -1 ..1]
        NumProduct numbers.Tail numbers.Head

let Problem34 = 
    let SumOfFactorsIsN n =
        n.ToString().ToCharArray() |> Array.map (fun c -> Factorial (Int32.Parse(c.ToString())))
                                   |> Array.sum
                                   |> fun x -> x = n

    [3..2540161] |> List.filter (fun x -> SumOfFactorsIsN x)
                 |> List.sum
// [/snippet]