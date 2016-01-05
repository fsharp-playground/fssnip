module Observable

open System
open System.Reactive
open System.Reactive.Linq

/// Generates an observable from an Async<_>.
let fromAsync computation = 
  Observable.Create(Func<_,_>(fun o ->
    if o = null then nullArg "observer"
    let cts = new System.Threading.CancellationTokenSource()
    let invoked = ref 0
    let cancelOrDispose cancel =
      if System.Threading.Interlocked.CompareExchange(invoked, 1, 0) = 0 then
        if cancel then cts.Cancel() else cts.Dispose()
    let wrapper = async {
      try
        let res = ref Unchecked.defaultof<_>
        try
          let! result = computation
          res := result
        with e -> o.OnError(e)
        o.OnNext(!res)
        o.OnCompleted()
      finally cancelOrDispose false }
    Async.StartImmediate(wrapper, cts.Token)
    Action(fun () -> cancelOrDispose true)))

/// Generates an observable from a tail-optimized loop, similar to AsyncSeq as defined at http://fssnip.net/1k
let fromAsyncSeries computation completed =
  Observable.Create (Func<_,_>(fun o ->
    if o = null then nullArg "observer"
    let cts = new System.Threading.CancellationTokenSource()
    let invoked = ref 0
    let cancelOrDispose cancel =
      if System.Threading.Interlocked.CompareExchange(invoked, 1, 0) = 0 then
        if cancel then cts.Cancel() else cts.Dispose()
    let wrapper = async {
      let rec next() = async {
        let res = ref Unchecked.defaultof<_>
        try
          let! result = computation
          res := result
        with e ->
          o.OnError(e)
          cancelOrDispose false

        if completed !res then
          o.OnCompleted()
          cancelOrDispose false
        else
          // May block the thread to prevent the continuation.
          // In high performance I/O operations, this should really
          // be an asynchronous process itself so that it will allow
          // the thread to be freed for other work.
          o.OnNext(!res)
          // May not want to call this continuation immediately
          return! next() }
      return! next() }
    Async.StartImmediate(wrapper, cts.Token)
    Action(fun () -> cancelOrDispose true)))
