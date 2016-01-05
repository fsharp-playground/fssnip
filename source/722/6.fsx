module BuilderExample
open System

//Example: nullable { ... } with combining functionality

type NullableBuilder() =
    let hasValue (a:Nullable<'a>) = a.HasValue 
    member t.Return(x) = Nullable(x)
    member t.Bind(x, rest) = 
        match hasValue x with 
        | false -> System.Nullable() //.NET null object
        | true -> rest(x.Value)

let nullable = NullableBuilder()

let test =
    nullable{
        let! a = System.Nullable(3) //"let!" calls builders Bind
        let! b = System.Nullable(5) //"let!" calls builders Bind

        //Inside computation expression(/monad): easy programming without Nullable-worries:
        let mult = a * b 
        let sum = mult + 1 

        return sum //"return" calls builders Return(x)
    }
//val test : Nullable<int> = 16


//------------------------------------------------------------------------------
// using e.g.
//     let! b = System.Nullable() 
// would cause to return:
//     val test : System.Nullable()

// own { ...} syntax is made with implementing Builder-class
// and (some of) the "interface" members

//------------------------------------------------
// Further reading:
// "Interface" described in: http://msdn.microsoft.com/en-us/library/dd233182.aspx
// More info: http://blogs.msdn.com/b/dsyme/archive/2007/09/22/some-details-on-f-computation-expressions-aka-monadic-or-workflow-syntax.aspx

// Check also Reactive Extensions 2.0 with F# observe { ... }:
// https://github.com/panesofglass/FSharp.Reactive/blob/master/src/Observable.fs

// and: http://fssnip.net/tags/computation+builder and http://fssnip.net/tags/monad

