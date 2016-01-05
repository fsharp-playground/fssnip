open System
open System.Threading.Tasks

type Microsoft.FSharp.Control.Async with
  static member Raise (e : #exn) = 
    Async.FromContinuations(fun (_,econt,_) -> econt e)

let awaitTask (t : System.Threading.Tasks.Task) =
  let flattenExns (e : AggregateException) = e.Flatten().InnerExceptions |> Seq.nth 0
  let rewrapAsyncExn (it : Async<unit>) =
    async { try do! it with :? AggregateException as ae -> do! (Async.Raise <| flattenExns ae) }
  let tcs = new TaskCompletionSource<unit>(TaskCreationOptions.None)
  t.ContinueWith((fun t' ->
    if t.IsFaulted then tcs.SetException(t.Exception |> flattenExns)
    elif t.IsCanceled then tcs.SetCanceled ()
    else tcs.SetResult(())), TaskContinuationOptions.ExecuteSynchronously)
  |> ignore
  tcs.Task |> Async.AwaitTask |> rewrapAsyncExn
    elif t.IsCanceled then tcs.SetCanceled ()
    else tcs.SetResult(())), TaskContinuationOptions.ExecuteSynchronously)
  |> ignore
  tcs.Task |> Async.AwaitTask