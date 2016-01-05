open System
open System.Collections

let calculate_primes max = 
    let array = new BitArray(max, true);
    let lastp = Math.Sqrt(float max) |> int
    for p in 2..lastp+1 do
        if array.Get(p) then
            for pm in p*2..p..max-1 do
                array.Set(pm, false);
    seq { for i in 2..max-1 do if array.Get(i) then yield i } 

let primes =
    let globalMax = 1000000
    let primes = calculate_primes globalMax |> Seq.cache
    fun max -> Seq.takeWhile (fun p -> p <= max) primes

let factor_by_trial_division n =
    let rec factor_by_trial_division' n primes =
        if n = 1 then 
            [1]
        else if Seq.isEmpty primes then
            [n]
        else
            let p = Seq.head primes
            if p*p > n then
                [n]
            else
                if n % p = 0 then
                    p :: factor_by_trial_division' (n/p) primes
                else
                    let primes' = Seq.skip 1 primes
                    factor_by_trial_division' n primes'
    factor_by_trial_division' n (primes (int(sqrt(float(n))) + 1))
