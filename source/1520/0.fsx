let data = [|1L..1000000L|]

// [snippet: Reactive Extensions (Rx)]
open System
open System.Reactive.Linq

let rxValue =
   data
      .ToObservable()
      .Where(fun x -> x%2L = 0L)
      .Select(fun x -> x * x)
      .Sum()      
      .ToEnumerable()
      |> Seq.head
// Real: 00:00:02.702, CPU: 00:00:02.812, GC gen0: 121, gen1: 2, gen2: 1
// [/snippet]

// [snippet: Observable module]
(* [omit:Observable module extensions] *)
module Observable =
   open System
   let ofSeq (xs:'T seq) : IObservable<'T> =
      { new IObservable<'T> with
         member __.Subscribe(observer) =
            for x in xs do observer.OnNext(x)
            observer.OnCompleted()
            { new IDisposable with member __.Dispose() = ()}           
      }
   let inline sum (observable:IObservable< ^T >) : IObservable< ^T >
         when ^T : (static member ( + ) : ^T * ^T -> ^T) 
         and  ^T : (static member Zero : ^T) = 
      { new IObservable<'T> with 
         member this.Subscribe(observer:IObserver<'T>) =
            let acc = ref (LanguagePrimitives.GenericZero)
            let accumulator =
               { new IObserver<'T> with 
                  member __.OnNext(x) = acc := !acc + x
                  member __.OnCompleted() = observer.OnNext(!acc)
                  member __.OnError(_) = failwith "Not implemented"
               }
            observable.Subscribe(accumulator)
      }
   let first (observable:IObservable<'T>) : 'T =
      let value = ref (Unchecked.defaultof<'T>)
      let _ = observable.Subscribe(fun x -> value := x)
      !value
(* [/omit] *)

let obsValue =
   data
   |> Observable.ofSeq
   |> Observable.filter (fun x -> x%2L = 0L)
   |> Observable.map (fun x -> x * x)
   |> Observable.sum
   |> Observable.first
// Real: 00:00:00.458, CPU: 00:00:00.453, GC gen0: 18, gen1: 1, gen2: 0
// [/snippet]

// [snippet: Nessos Streams]
open Nessos.Streams

let streamValue =
   data
   |> Stream.ofArray
   |> Stream.filter (fun x -> x%2L = 0L)
   |> Stream.map (fun x -> x * x)
   |> Stream.sum
// Real: 00:00:00.119, CPU: 00:00:00.109, GC gen0: 0, gen1: 0, gen2: 0
// [/snippet]