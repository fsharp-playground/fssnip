// [snippet:StartDisposable extension]
open System

type Microsoft.FSharp.Control.Async with
  /// Starts the specified operation using a new CancellationToken and returns
  /// IDisposable object that cancels the computation. This method can be used
  /// when implementing the Subscribe method of IObservable interface.
  static member StartDisposable(op:Async<unit>) =
    let ct = new System.Threading.CancellationTokenSource()
    Async.Start(op, ct.Token)
    { new IDisposable with 
        member x.Dispose() = ct.Cancel() }
// [/snippet]

// [snippet:Creating counter observable]
/// Creates IObservable that fires numbers with specified delay
let createCounterEvent (delay) =
  /// Recursive async computation that triggers observer
  let rec counter (observer:IObserver<_>) count = async {
    do! Async.Sleep(delay)
    observer.OnNext(count)
    return! counter observer (count + 1) }
  // Return new IObservable that starts 'counter' using
  // 'StartDisposable' when a new client subscribes.
  { new IObservable<_> with
      member x.Subscribe(observer) =
        counter observer 0
        |> Async.StartDisposable }

// Start the counter with 1 second delay and print numbers
let disp = 
  createCounterEvent 1000
  |> Observable.map (sprintf "Count: %d")
  |> Observable.subscribe (printfn "%s")

// Dispose of the observer (and underlying async)
disp.Dispose()
// [/snippet]