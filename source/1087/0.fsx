// based on http://stackoverflow.com/a/11191070

open System
open System.Threading
open System.Threading.Tasks

type Microsoft.FSharp.Control.Async with
    static member AwaitTask (t : Task<'T>, timeout : int) =
        async {
            use cts = new CancellationTokenSource()
            use timer = Task.Delay (timeout, cts.Token)
            try
                let! completed = Async.AwaitTask <| Task.WhenAny(t, timer)
                if completed = (t :> Task) then
                    let! result = Async.AwaitTask t
                    return Some result
                else return None

            finally cts.Cancel()
        }

// example
Async.AwaitTask((Task.Delay 500).ContinueWith(fun _ -> 42), 600) |> Async.RunSynchronously