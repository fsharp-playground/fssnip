// [snippet:LazyBuilder]
module Lazy =

  type LazyBuilder () =
    let force (x : Lazy<'T>) = x |> function | Lazy x -> x
    /// bind
    let (>>=) (x:Lazy<'a>) (f:('a -> Lazy<'a>)) : Lazy<'a> =
      lazy (x |> force |> f |> force)  
    /// zero
    let lazyzero = Seq.empty
    
    member this.Bind(x, f) = x >>= f
    member this.Return (x) = lazy x
    member this.ReturnFrom (x) = x
    member this.Zero() = lazyzero 
    member this.Delay f = f()

    member this.Combine (a,b) = 
      Seq.append a b 

    member this.Combine (a,b) = 
      Seq.append (Seq.singleton a) b

    member this.Combine (a,b) = 
      Seq.append a (Seq.singleton b) 

    member this.Combine (Lazy(a), Lazy(b)) = 
      lazy Seq.append a b 

    member this.Combine (a:Lazy<'T>, b:Lazy<seq<'T>>) = 
      (a,b) |> function | (Lazy x, Lazy y) -> lazy Seq.append (Seq.singleton x) y

    member this.Combine (a:Lazy<seq<'T>>, Lazy(b)) = 
      a |> function | Lazy x -> lazy Seq.append x (Seq.singleton b) 

    member this.Combine (a:Lazy<seq<'T>>, b:seq<Lazy<'T>>) =
      let notlazy (xs:seq<Lazy<'T>>) = xs |> Seq.map (fun (Lazy(a)) -> a)
      a |> function | Lazy x -> lazy Seq.append x (notlazy b) 

    member this.Combine (a:seq<Lazy<'T>>, b:Lazy<seq<'T>>) =
      let notlazy (xs:seq<Lazy<'T>>) = xs |> Seq.map (fun (Lazy(a)) -> a)
      b |> function | Lazy x -> lazy Seq.append (notlazy a) x  

    member this.For (s,f) =
      s |> Seq.map f 

    member this.Yield x = lazy x
    member this.YieldFrom x = x

    member this.Run (x:Lazy<'T>) = x
    member this.Run (xs:seq<Lazy<unit>>) = 
      xs |> Seq.reduce (fun a b -> this.Bind(a, fun _ -> b)) 
    member this.Run (xs:seq<Lazy<'T>>) = 
      xs |> Seq.map (fun (Lazy(a)) -> a) |> fun x -> lazy x

  let lazyz = LazyBuilder () 
  let lifM f (Lazy(a)) = lazy f a   
// [/snippet]

// [snippet:Sample]
open System
open Lazy
let result = 
  lazyz { let! a = lazy ("Hello,")
          let! b = lazy ("World!")
          return a + b}
result.Force() |> (printfn "%s") |> ignore

let lazyp n = lazy (printfn "%d" n)
let result2 = 
  let a = 
    lazyz {
        yield! lazyp 2
        ()}

  lazyz {
    yield! lazyp 0
    yield! lazyp 1
    yield! a
    for i in 3..5 do
      yield! lazyp i 
    yield! lazyp 6
    yield! lazyz {
              yield! lazyp 7
              yield! lazyp 8
              ()}
    for i in 9..10 do
      yield! lazyp i 
    yield! seq {for n in [11..15] -> lazyp n}
    yield! lazyz {
              yield! lazyp 16
              yield! lazyp 17
              ()}
    yield! lazyz {
              yield! lazyp 18
              yield! lazyp 19
              ()}
    yield! lazyp 20
    ()  }
result2.Force() 
Console.ReadLine () |> ignore
// [/snippet]