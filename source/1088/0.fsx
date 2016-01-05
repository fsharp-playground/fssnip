// inspired by http://stackoverflow.com/a/11191070

open System

type private Completed<'T>(value : 'T) =
    inherit Exception()
    member __.Value = value

exception private Timeout

type Async with
    static member CancelAfter timeout (f : Async<'T>) =
        let econt e = Async.FromContinuations(fun (_,econt,_) -> econt e)
        let worker = async {
            let! r = f
            return! econt <| Completed(r)
        }
        let timer = async {
            do! Async.Sleep timeout
            return! econt Timeout
        }

        async {
            try 
                let! _ = Async.Parallel [worker ; timer]
                return failwith "unreachable exception reached."
            with 
            | :? Completed<'T> as t -> return Some t.Value
            | Timeout -> return None
        }

// example
Async.Sleep 400 |> Async.CancelAfter 500 |> Async.RunSynchronously
Async.Sleep 400 |> Async.CancelAfter 300 |> Async.RunSynchronously