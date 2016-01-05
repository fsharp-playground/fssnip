// [snippet:Link to blog post]
// Script example (3 of 3) for Monad blog post
// at http://rodhern.wordpress.com/2014/02/
// [/snippet]
open System

/// Today's date and time for use in examples below.
let today = DateTime.Now

/// The test parameter I will use is "2" as in "two weeks from now".
let myInputParam = 2


// ---- ---- ----
// -- The Monad example of function composition (Bind)
// ---- ---- ----

/// In this example, as is often the case, the Monad, M<'V>,
/// is a container for, 'V, the value if interest.
// [snippet:The Monad defined] 
type M<'V> =
     {
       Value: 'V
       LogMessages: string list
     }
     with
      static member StaticReturn v = { Value= v; LogMessages= [] }
      static member StaticBind (mt: M<'a>) (fn: 'a -> M<'b>) =
              let result = fn mt.Value
              let newmessage = sprintf ("Passing value %+A") mt.Value
              let logmessages = result.LogMessages @ (newmessage :: mt.LogMessages)
              { Value= result.Value; LogMessages= logmessages }
// [/snippet]
// [snippet:Some Monad sugar]
/// A shorter name for M.StaticReturn.
/// Turning a variable of type 'a into M<'a> .
let ret x = M<_>.StaticReturn x

/// Turning a function with signature 'a -> 'b  into  M<'a> -> M<'b> .
let package fn mt = M<_>.StaticBind mt (fn >> ret)

/// The builder type may be an empty class like this one.
/// Often the builder instance is really nothing more than
/// a syntactic requirement.
type ComputationExpressionBuilderType () =
      member public self.Return v = M<_>.StaticReturn v
      member public self.Bind (mt, fn) = M<_>.StaticBind mt fn
// [/snippet]
/// Our good old compose function from the first example
let compose (fun1, fun2) = fun arg -> fun2 (fun1 (arg))
(* FSSnip is not impressed with this notation ;-)
type A = int       type R = M<A>
type B = float     type S = M<B>
type C = DateTime  type T = M<C>
type D = string    type U = M<D>
*)
// [snippet:Types]
type A = int       
type B = float     
type C = DateTime  
type D = string    

type R = M<A>
type S = M<B>
type T = M<C>
type U = M<D>
// [/snippet]
// Functions from first example ( f: A -> B,  g: B -> C,  g: C -> D)
// [snippet:Functions (first example repeated)]
let f (noWeeks: A): B = float (7 * noWeeks)
let g (noDays: B): C = today.AddDays noDays
let h (date: C): D = date.ToShortDateString ()
// [/snippet]
// The same functions packaged ( pf: R -> S,  pg: S -> T,  h: T -> U)
// [snippet:Functions (packaged)]
let pf: R -> S = package f
let pg: S -> T = package g
let ph: T -> U = package h
// [/snippet]
// The actual composition of the three functions
// [snippet:Function composition]
let fghOne: R -> U = compose(compose(pf, pg), ph)
let fghTwo: R -> U = compose(pf, compose(pg, ph))
// [/snippet]
// [snippet:Some results (packaged functions)]
let fghOneResult = fghOne (ret myInputParam)
let fghTwoResult = fghTwo (ret myInputParam)
// [/snippet]
// The preparation (Mf: A -> S, Mg: B -> T, Mg: C -> U)
// [snippet:Getting ready for computation expression]
let Mx: R = ret myInputParam
let Mf: A -> S = f >> M<_>.StaticReturn
let Mg: B -> T = g >> M<_>.StaticReturn
let Mh: C -> U = h >> M<_>.StaticReturn
// [/snippet]
// Example of calling functions using computation expression syntax

/// By the construction of the syntax the builder must be an instance
/// of the builder type. The static methods will not do by themselves.
// [snippet:Some results (computation expression)]
let builder = new ComputationExpressionBuilderType ()

let result =
    builder {
              let! a = Mx
              let! b = Mf a
              let! c = Mg b
              let! d = Mh c
              return d
            }
// [/snippet]
