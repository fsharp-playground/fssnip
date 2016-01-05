module AsyncPrimitives

open System
open System.Collections.Generic
open System.Threading

// [snippet: Async primitives]
type AsyncManualResetEvent() =
  let resultCell = ref <| AsyncResultCell<_>()

  member x.AsyncWait() = (!resultCell).AsyncResult
  member x.Set() = (!resultCell).RegisterResult(AsyncOk(true))
  member x.Reset() =
    let rec swap newVal =
      let currentValue = !resultCell
      let result = Interlocked.CompareExchange<_>(resultCell, newVal, currentValue)
      if obj.ReferenceEquals(result, currentValue) then () else
      Thread.SpinWait 20
      swap newVal
    swap <| AsyncResultCell<_>()

type AsyncAutoResetEvent(?reuseThread) =
  let mutable awaits = Queue<_>()
  let mutable signalled = false
  let completed = async.Return true
  let reuseThread = defaultArg reuseThread false

  member x.AsyncWait() =
    lock awaits (fun () ->
      if signalled then
        signalled <- false
        completed
      else
        let are = AsyncResultCell<_>()
        awaits.Enqueue are
        are.AsyncResult )

type AsyncCountdownEvent(initialCount) =
  let amre = AsyncManualResetEvent()
  do if initialCount <= 0 then raise (new ArgumentOutOfRangeException("initialCount"))
  let count = ref initialCount

  member x.AsyncWait() = amre.AsyncWait()
  member x.Signal() =
    if !count <= 0 then invalidOp ""

    let newCount = Interlocked.Decrement(count)
    if newCount = 0 then
      amre.Set()
    elif newCount < 0 then
      invalidOp ""
    else ()

  member x.SignalAndWait() =
    x.Signal()
    x.AsyncWait()
// [/snippet]

#if INTERACTIVE
#r @"E:\Code\AsyncPrimitives\packages\FSPowerPack.Core.Community.2.0.0.0\Lib\Net40\FSharp.PowerPack.dll"
#load "AsyncPrimitives.fs"
#endif

open System
open System.Threading
open AsyncPrimitives

// [snippet: Sample usage]
Console.WriteLine("AsyncManualResetEvent")

let amre = AsyncManualResetEvent()
let x = async{let! x = amre.AsyncWait()
              Console.WriteLine("First signalled")}

let y = async{let! x = amre.AsyncWait()
             Console.WriteLine("Second signalled")}

let z = async{let! x = amre.AsyncWait()
              Console.WriteLine("Third signalled")}
//start async workflows x and y
Async.Start x
Async.Start y

//reset the asyncManualResetEvent, this will test whether the async workflows x and y 
// are orphaned due to the AsyncResultCell being recycled.
amre.Reset()

//now start the async z
Async.Start z

//we set a single time, this should result in the three async workflows completing
amre.Set()

Console.WriteLine()

Console.WriteLine("AsyncCountdownEvent")

let ace = AsyncCountdownEvent(3)

let aceWait1 = async{
  let! _ = ace.SignalAndWait()
  Console.WriteLine("First signalled")
}

let aceWait2 = async{
  let! _ = ace.SignalAndWait()
  Console.WriteLine("Second signalled")
}

let aceWait3 = async {
  let! _ = ace.SignalAndWait()
  Console.WriteLine("Third signalled")
}

//start async workflows aceWait1 and aceWait2
Async.Start aceWait1
Async.Start aceWait2
Async.Start aceWait3
// [/snippet]

Console.WriteLine()

Console.ReadLine() |> ignore