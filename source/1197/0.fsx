//[snippet:implimentation]
open System
open System.Threading.Tasks

module Async = 
  let map f (m:_ Async) = async { let! x = m in return f x }

open FSharp.Control.Observable
type AsyncBuilder with
  member __.Bind (m:'a IObservable, f:'a -> 'b Async) = __.Bind(Async.AwaitObservable m, f)
  member __.Bind (m:'a Task, f:'a -> 'b Async) = __.Bind(Async.AwaitTask m, f)
  member __.Bind (m:int, f: unit -> 'a Async) =
    async {
    do! Async.Sleep m
    return! f () }
  member __.For(xsm: #seq<_> Async, f:_ ->_) = Async.map (Seq.map f) xsm
  member __.Yield x = x
//[/snippet]

//[snippet:usage]
let event = Event<string>()
let ievent      : string IEvent      = event.Publish
let iobservable : int IObservable = Observable.map String.length ievent

let unitEvent = Event<unit>()
let unitIEvent      : unit IEvent      = unitEvent.Publish
let unitIObservable : unit IObservable = unitIEvent :> unit IObservable


async {           
// let! str = Async.AwaitEvent ievent
let! str = ievent
// let! len = Async.AwaitObservable iobservable
let! len = iobservable
// do! unitIEvent |> Async.AwaitEvent
do! unitIEvent
// do! unitIObservable |> Async.AwaitObservable
do! unitIObservable
// do! Async.Sleep 100
do! 100
printfn "%s's length is %d" str len 

let ism = async { return [ 1 .. 10 ] }
// let! is = ism
// return (seq { for i in is -> i * 2 })
for i in ism -> i * 2
}
|> Async.map(printfn "%A")
|> Async.StartImmediate


event.Trigger "triggered first time"
event.Trigger "triggered second time"
unitEvent.Trigger ()
unitEvent.Trigger ()
// [/snippet]

// [snippet:result]
// triggered first time's length is 21
// seq [2; 4; 6; 8; ...]
// [/snippet]