module BuilderExample
open System

//------------------------------------------------
//Example 1: First try, just return

type Nullable1Builder() =
    member this.Return(x) = Nullable(x)

let myNullable1 = Nullable1Builder()

let MakeNullable =
    myNullable1{
        let a = 5 + 1
        return a // calls builders Return(x)
    }
//val test : Nullable<int> = 6

//------------------------------------------------
//Example 2: Just return

type Nullable2Builder() =
    let hasValue (a:Nullable<'a>) = a.HasValue 
    member t.Return(x) = Nullable(x)
    member t.Bind(x, rest) = 
        match hasValue x with 
        | false -> System.Nullable() 
        | true -> rest(x.Value)

let nullable = Nullable2Builder()

let test =
    nullable{
        let! a = System.Nullable(4) // Call Bind
        let! b = System.Nullable(5) // Call Bind

        //Inside computation expression(/monad): easy programming without Nullable-worries:
        let mult = a * b 
        let sum = mult + 1 

        return sum //Call Return
    }
//val test : Nullable<int> = 16

//------------------------------------------------------------------------------
// using e.g.
//     let! b = System.Nullable() 
// would cause to return:
//     val test : System.Nullable()

// own { ...} syntax is made with implementing Builder-class
// and (some of) the "interface" members

//------------------------------------------------------------------------------
// Some optional technical details:
// Actual stack in example2, you don't have to care about this:

// makeString.Bind(Nullable(3), (fun res1 ->
//   makeString.Bind(Nullable(5), (fun res2 ->
//     makeString.Let(res1*res2, (fun res3 ->
//       makeString.Let(res3+1, (fun res4 ->
//         makeString.Return(res4)))))))))

//------------------------------------------------

// Further reading:
// Interface described in: http://msdn.microsoft.com/en-us/library/dd233182.aspx
// More info: http://blogs.msdn.com/b/dsyme/archive/2007/09/22/some-details-on-f-computation-expressions-aka-monadic-or-workflow-syntax.aspx

// Check also Reactive Extensions 2.0 with F# observe { ... }:
// https://github.com/panesofglass/FSharp.Reactive/blob/master/src/Observable.fs

// and: http://fssnip.net/tags/computation+builder and http://fssnip.net/tags/monad

//------------------------------------------------
(*[omit:(Some more optional technical details...)]*)
//<just for intellisense>
type M<'T> = System.Collections.Generic.IEnumerable<'T>
type ImplementJustWhatYouWantBuilder = 
//</just for intellisense>

// The builder methods are described in MSDN:

      abstract member Bind:  M<'T> * ('T -> M<'U>) -> M<'U>  // Called for let! and do! in computation expressions.
      abstract member Delay:  (unit -> M<'T>) -> M<'T> // Wraps a computation expression as a function.
      abstract member Return:  'T -> M<'T> // Called for return in computation expressions.
      abstract member ReturnFrom: M<'T> -> M<'T> // Called for return! in computation expressions.
      abstract member Run:  M<'T> -> M<'T>  // Executes a computation expression.
// or abstract member Run:  M<'T> -> 'T //  Executes a computation expression.
      abstract member Combine: M<'T> * M<'T> -> M<'T> // Called for sequencing in computation expressions.
      abstract member Combine: M<unit> * M<'T> -> M<'T> // Called for sequencing in computation expressions. 
      abstract member For:  seq<'T> * ('T -> M<'U>) -> M<'U>  // Called for for...do expressions in computation expressions.
// or abstract member For:  seq<'T> * ('T -> M<'U>) -> seq<M<'U>> // Called for for...do expressions in computation expressions.
      abstract member TryFinally: M<'T> * (unit -> unit) -> M<'T> // Called for try...finally expressions in computation expressions.
      abstract member TryWith: M<'T> * (exn -> M<'T>) -> M<'T> // Called for try...with expressions in computation expressions.
      abstract member Using:  'T * ('T -> M<'U>) -> M<'U> when 'U :> IDisposable // Called for use bindings in computation expressions.
      abstract member While:  (unit -> bool) * M<'T> -> M<'T> // Called for while...do expressions in computation expressions.
      abstract member Yield:  'T -> M<'T> // Called for yield expressions in computation expressions.
      abstract member YieldFrom: M<'T> -> M<'T> // Called for yield! expressions in computation expressions.
      abstract member Zero:  unit -> M<'T> // Called for empty else branches of if...then expressions in computation expressions.


