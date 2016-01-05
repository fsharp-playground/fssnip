let isPrime num sieve =
    sieve |> List.forall (fun x -> num % x <> 0)

let prime upper =
    let rec makeSieve ls acc =
        match ls with
        | x::xs when isPrime x acc -> makeSieve xs (x::acc)
        | x::xs                    -> makeSieve xs acc
        | _                        -> acc
    let nums = [2..upper]
    makeSieve nums []