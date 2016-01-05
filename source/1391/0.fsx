open System

let rng = new Random()

let Shuffle (org:_[]) = 
    let arr = Array.copy org
    let max = (arr.Length - 1)
    let randomSwap (arr:_[]) i =
        let pos = rng.Next(max)
        let tmp = arr.[pos]
        arr.[pos] <- arr.[i]
        arr.[i] <- tmp
        arr
   
    [|0..max|] |> Array.fold randomSwap arr


[|1..30|] |> Shuffle |> printfn "%A"
[|"F#";"C#";"Haskell";"JavaScript";"Erlang"|] |> Shuffle |> printfn "%A"