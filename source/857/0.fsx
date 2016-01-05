type Microsoft.FSharp.Control.Async with
    static member Raise (e : #exn) = 
        Async.FromContinuations(fun (_,econt,_) -> econt e)

#time
async {
    for i in 1 .. 1000000 do
        try
            return raise <| new System.Exception()
        with ex -> ()
} |> Async.RunSynchronously

async {
    for i in 1 .. 1000000 do
        try
            return! Async.Raise <| new System.Exception()
        with ex -> ()
} |> Async.RunSynchronously