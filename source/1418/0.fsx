let sieve limit =
    let range = {2 .. limit}
    let nonPrimes = Seq.collect(fun x-> Seq.filter(function |s when s <> x -> s % x = 0 | _ -> false) range) range
    Seq.filter(fun f -> not (Seq.exists(fun np -> np = f) nonPrimes)) range