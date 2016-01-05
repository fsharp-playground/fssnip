open System

type Async<'a> with
  member this.ToObservable() =
    { new IObservable<_> with
        member x.Subscribe(o) =
          if o = null then nullArg "observer"
          let cts = new System.Threading.CancellationTokenSource()
          let invoked = ref 0
          let cancelOrDispose cancel =
            if System.Threading.Interlocked.CompareExchange(invoked, 1, 0) = 0 then
              if cancel then cts.Cancel() else cts.Dispose()
          let wrapper = async {
            try
              try
                let! r = this
                o.OnNext(r)
                o.OnCompleted()
              with e -> o.OnError(e)
            finally cancelOrDispose false }
          Async.StartImmediate(wrapper, cts.Token)
          { new IDisposable with member x.Dispose() = cancelOrDispose true } }