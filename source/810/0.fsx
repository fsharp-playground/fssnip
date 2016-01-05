

open System

/// Helper that can be used for writing CPS-style code that resumes
/// on the same thread where the operation was started.
let internal synchronize f = 
  let ctx = System.Threading.SynchronizationContext.Current 
  f (fun g ->
    let nctx = System.Threading.SynchronizationContext.Current 
    if ctx <> null && ctx <> nctx then ctx.Post((fun _ -> g()), null)
    else g() )

type Microsoft.FSharp.Control.Async with 

  /// Behaves like AwaitObservable, but calls the specified guarding function
  /// after a subscriber is registered with the observable.
  static member GuardedAwaitObservable (ev1:IObservable<'T1>) guardFunction =
    synchronize (fun f ->
      Async.FromContinuations((fun (cont,econt,ccont) -> 
        let rec finish cont value = 
          remover.Dispose()
          f (fun () -> cont value)
        and remover : IDisposable = 
          ev1.Subscribe
            ({ new IObserver<_> with
                  member x.OnNext(v) = finish cont v
                  member x.OnError(e) = finish econt e
                  member x.OnCompleted() = 
                    let msg = "Cancelling the workflow, because the Observable awaited using AwaitObservable has completed."
                    finish ccont (new System.OperationCanceledException(msg)) }) 
        guardFunction() )))

let loop = async {
  for i in 0 .. 9999 do
    printfn "Starting: %d" i
    do! Async.Sleep(1000)
    printfn "Done: %d" i }

open System.Threading

module Async = 
  let StartCancellable work = async {
    let cts = new CancellationTokenSource()
    let evt = new Event<_>()
    Async.Start(Async.TryCancelled(work, ignore >> evt.Trigger), cts.Token)
    return Async.GuardedAwaitObservable evt.Publish cts.Cancel }
  
async { let! cancelToken = Async.StartCancellable(loop)
        printfn "started"
        do! Async.Sleep(5500)
        printfn "cancelling"
        do! cancelToken
        printfn "done" }
|> Async.Start