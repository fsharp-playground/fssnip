
let EvenFibSeqForNTerms count =
    let mutable before = ref 0
    let mutable next = ref 1
    let mutable temp = ref 0
    let mutable fib = ref Seq.init
    seq {for i in 1 .. count do
            let _ = temp <- next
            let _ = next <- !before + !next
            let _ = before <- temp
            if !next % 2 = 0 then yield !next}

printfn "%A" (EvenFibSeqForNTerms 10)
    