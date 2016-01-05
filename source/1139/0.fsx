open System
open System.Threading.Tasks

type Microsoft.FSharp.Control.Async with
  static member Raise (e : #exn) = 
    Async.FromContinuations(fun (_,econt,_) -> econt e)

let awaitTask (t : System.Threading.Tasks.Task) =
  let flattenExns (e : AggregateException) = e.Flatten().InnerExceptions |> Seq.nth 1
  let rewrapAsyncExn (it : Async<unit>) =
    async { try do! it with :? AggregateException as ae -> return! flattenExns ae |> Async.Raise }
  let tcs = new TaskCompletionSource<unit>()
  t.ContinueWith((fun t' ->
    if t.IsFaulted then tcs.SetException(t.Exception |> flattenExns)
    elif t.IsCanceled then tcs.SetCanceled ()
    else tcs.SetResult(())), TaskContinuationOptions.ExecuteSynchronously)
  |> ignore
  tcs.Task |> Async.AwaitTask