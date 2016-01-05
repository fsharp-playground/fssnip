let EvenFibSeqForNTerms count =
    let rec fib acc before next =
        seq {
            while acc < count do
                let next' = before + next
                if next' % 2 = 0 then 
                    yield next'
                yield! fib (acc + 1) next next' }
    fib 0 0 1
