open System.Threading

type Async with
    static member Choice(tasks : Async<'T option> seq) : Async<'T option> =
        Async.FromContinuations(fun (cont,econt,_) ->
            let tasks = Seq.toArray tasks
            if tasks.Length = 0 then cont None else

            let cts = new CancellationTokenSource()
            let count = ref tasks.Length
            let completed = ref false
            let synchronize f =
                lock count (fun () ->
                    if !completed then ()
                    else f ())

            let wrap task =
                async {
                    try
                        let! res = task
                        match res with
                        | None -> 
                            synchronize (fun () ->
                                decr count
                                if !count = 0 then 
                                    cont None
                                    cts.Dispose ())
                        | Some r ->
                            synchronize (fun () ->
                                cont (Some r)
                                cts.Cancel()
                                completed := true)
                    with e -> 
                        synchronize (fun () ->
                            econt e
                            cts.Cancel()
                            completed := true)
                }

            for task in tasks do
                Async.Start(wrap task, cts.Token)
        )

// example 1    

let task delay result =
    async {
        do! Async.Sleep delay
        do! async.Zero () // force ct check here
        do printfn "returning %A after %d ms" result delay
        return result
    }

Async.Choice
    [ 
        task 100 None 
        task 200 (Some 1) 
        task 500 (Some 2) 
        task 1000 (Some 3)
    ] |> Async.RunSynchronously

// example 2

/// parallel existential combinator
let exists (f : 'T -> Async<bool>) inputs =
    let wrapper t =
        async {
            let! res = f t
            if res then return Some ()
            else return None
        }

    async {
        let! res = 
                inputs
                |> Seq.map wrapper
                |> Async.Choice

        return res.IsSome
    }