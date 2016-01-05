open System.Collections
open System.Collections.Generic

// [snippet:Module with basic functions for working with IEnumerator]
module Enumerator = 

  let single a = 
    let state = ref 0
    { new IEnumerator<_> with 
        member self.Current = if (state.Value = 1) then a else invalidOp "!"
      interface System.Collections.IEnumerator with
        member self.Current = if (state.Value = 1) then box a else invalidOp "!"
        member self.MoveNext() = state := state.Value + 1; state.Value = 1
        member self.Reset() = state := 0
      interface System.IDisposable with 
        member self.Dispose() = ()}

  let combine (a:IEnumerator<_>) b = 
    let current = ref a
    let first = ref true
    { new IEnumerator<_> with 
        member self.Current = current.Value.Current
      interface System.Collections.IEnumerator with
        member self.Current = box current.Value.Current
        member self.MoveNext() = 
          if current.Value.MoveNext() then true 
          elif first.Value then 
            current := b
            first := false
            current.Value.MoveNext()
          else false
        member self.Reset() = 
          a.Reset(); b.Reset()
          first := true; current := a
      interface System.IDisposable with 
        member self.Dispose() = a.Dispose(); b.Dispose() }
  
  let zero () = 
    { new IEnumerator<_> with 
        member self.Current = invalidOp "!"
      interface System.Collections.IEnumerator with
        member self.Current = invalidOp "!"
        member self.MoveNext() = false
        member self.Reset() = ()
      interface System.IDisposable with 
        member self.Dispose() = ()}
  
  let delay f = 
    let en : Lazy<IEnumerator<_>> = lazy f()
    { new IEnumerator<_> with 
        member self.Current = en.Value.Current
      interface System.Collections.IEnumerator with
        member self.Current = box en.Value.Current
        member self.MoveNext() = en.Value.MoveNext()
        member self.Reset() = en.Value.Reset()
      interface System.IDisposable with 
        member self.Dispose() = en.Value.Dispose() }

  let toSeq gen = 
    { new IEnumerable<'T> with 
          member x.GetEnumerator() = gen() 
      interface IEnumerable with 
          member x.GetEnumerator() = (gen() :> IEnumerator) }
// [/snippet]

// [snippet:Computation builder for working with enumerators]
type EnumerableBuilder() = 
  member x.Delay(f) = Enumerator.delay f
  member x.YieldFrom(a) = a
  member x.Yield(a) = Enumerator.single a
  member x.Bind(a:IEnumerator<'a>, f:_ -> IEnumerator<'b>) =
    if (a.MoveNext()) then f (Some(a.Current)) else f(None) 
  member x.Combine(a, b) = Enumerator.combine a b
  member x.Zero() = Enumerator.zero ()

let iter = new EnumerableBuilder()    
// [/snippet]

// [snippet:Examples of using 'iter' computations]
// Enumerator that produces 3 numbers
let e = iter { yield 1; yield 2; yield 3 }
e.MoveNext()
e.Current

// Enumerator that iumplements "identity" and prints elements
let rec loop e = iter {
  let! v = e
  printfn "%A" v
  yield v
  yield! loop e }
  
let e2 = iter { yield 1; yield 2; yield 3 }
let r = loop e2

r.MoveNext()
r.Current

// Implementing the zip function for enumerators
let rec zipE xs ys = iter {
  let! x = xs
  let! y = ys
  match x, y with 
  | Some(x), Some(y) ->
    yield x, y
    yield! zipE xs ys 
  | _ -> () }

// Implementing zip for sequences using zip for enumerators
let zip (a:seq<_>) (b:seq<_>) = 
  Enumerator.toSeq (fun () -> 
    zipE (a.GetEnumerator()) (b.GetEnumerator()))

zip [1;2;3] ["a";"b";"c"]
zip [1;2;3;4] ["a";"b";"c"]
// [/snippet]
let rec map f en = iter {
  let! x = en
  match x with 
  | Some(x) -> yield f x
               yield! map f en
  | _ -> () }               

// Implementing zip for sequences using zip for enumerators
let zipWithFun f g h (a:seq<_>) (b:seq<_>) = 
  let rec zipWithFunE xs ys = iter {
    let! x = xs
    let! y = ys
    match x, y with 
    | Some(x), Some(y) -> 
        yield f (x, y)
        yield! zipWithFunE xs ys 
    | Some(rest), _ ->
        yield g rest
        yield! xs |> map g
    | _, Some(rest) ->
        yield h rest  
        yield! ys |> map h
    | _ -> () }

  Enumerator.toSeq (fun () -> 
    zipE (a.GetEnumerator()) (b.GetEnumerator()))