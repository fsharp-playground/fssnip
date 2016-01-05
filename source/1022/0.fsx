open System.Threading

type Async with
    static member Isolate(f : CancellationToken -> Async<'T>) : Async<'T> =
        async {
            let! ct = Async.CancellationToken
            let isolatedTask = Async.StartAsTask(f ct)
            return! Async.AwaitTask isolatedTask
        }

// example

let rec loop (ct : CancellationToken) : Async<unit> =
    async {
        printfn "looping..."
        
        if ct.IsCancellationRequested then
            printfn "external ct has been cancelled, exiting.."
            return ()
        else
            do! Async.Sleep 1000
            return! loop ct
    }

let cts = new CancellationTokenSource()

Async.Start(Async.Isolate loop, cancellationToken = cts.Token)

cts.Cancel()