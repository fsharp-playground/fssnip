#r @"C:\Program Files (x86)\Microsoft Reactive Extensions SDK\v1.1.10621\Binaries\.NETFramework\v4.0\System.Reactive.dll"

// When subscribed to, generates numbers every 100ms
let createCounter () =
  let evt = new Event<_>()
  async { for i in 0 .. 100 do 
            evt.Trigger(i)
            do! Async.Sleep(100) }
  |> Async.Start
  evt.Publish

open System.Reactive.Linq

// Create sliding window of size 3
createCounter().Window(3, 1)
|> Observable.subscribe (fun obs ->
    // If we don't subscribe to the observer immediately, then
    // we loose some values that were produced in the meantime.
    // This is a bit unfortunate as we may want to do some operation
    /// here and it may take some time...
    async { do! Async.Sleep(300)
            obs.Aggregate("", fun ac s -> sprintf "%s %d," ac s) 
            |> Observable.add (printfn ">> %s") }
    |> Async.Start )