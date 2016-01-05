module Async =
    /// Run two async's in parallel in the thread pool and return the results together, asynchronously
    let Parallel2 (p1, p2) = 
        async { let! job1 = Async.StartChild p1
                let! job2 = Async.StartChild p2
                let! res1 = job1
                let! res2 = job2
                return (res1, res2) }

    /// Run three async's in parallel in the thread pool and return the results together, asynchronously
    let Parallel3 (p1, p2, p3) = 
        async { let! job1 = Async.StartChild p1
                let! job2 = Async.StartChild p2
                let! job3 = Async.StartChild p3
                let! res1 = job1
                let! res2 = job2
                let! res3 = job3
                return (res1, res2, res3) }

    let private boxp p = async { let! res = p in return box res }

    /// Alternative version of Async.Parallel2
    let Parallel2b (p1:Async<'T1>, p2:Async<'T2>) : Async<'T1 * 'T2> = 
        async { let! results = Async.Parallel [| boxp p1; boxp p2 |]
                return (unbox results.[0],unbox results.[1]) }

    /// Alternative version of Async.Parallel3
    let Parallel3b (p1:Async<'T1>, p2:Async<'T2>, p3:Async<'T3>) : Async<'T1 * 'T2 * 'T3> = 
        let boxp p = async { let! res = p in return box res }
        async { let! results = Async.Parallel [| boxp p1; boxp p2; boxp p3 |]
                return (unbox results.[0], unbox results.[1], unbox results.[2]) }

