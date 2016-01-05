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
//Example 2: nullable { ... } with combining functionality

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

// nullable.Bind(Nullable(3), (fun res1 ->
//   nullable.Bind(Nullable(5), (fun res2 ->
//     nullable.Let(res1*res2, (fun res3 ->
//       nullable.Let(res3+1, (fun res4 ->
//         nullable.Return(res4)))))))))



//------------------------------------------------

// Further reading:
// Interface described in: http://msdn.microsoft.com/en-us/library/dd233182.aspx
// More info: http://blogs.msdn.com/b/dsyme/archive/2007/09/22/some-details-on-f-computation-expressions-aka-monadic-or-workflow-syntax.aspx

// Check also Reactive Extensions 2.0 with F# observe { ... }:
// https://github.com/panesofglass/FSharp.Reactive/blob/master/src/Observable.fs

// and: http://fssnip.net/tags/computation+builder and http://fssnip.net/tags/monad


