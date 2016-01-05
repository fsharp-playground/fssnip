// [snippet:Link to blog post]
// Script example (1 of 3) for Monad blog post
// at http://rodhern.wordpress.com/2014/02/
// [/snippet]
open System

/// Today's date and time for use in examples below.
let today = DateTime.Now

/// The test parameter I will use is "2" as in "two weeks from now".
let myInputParam = 2


// ---- ---- ----
// -- The simplest example of function composition
// ---- ---- ----

// The preparation (f: A -> S, g: B -> T, g: C -> U)
// [snippet:Types]
type A = int
type S = float
type B = S
type T = DateTime
type C = T
type U = string
// [/snippet]
// [snippet:Functions]
let f (noWeeks: A): S = float (7 * noWeeks)
let g (noDays: B): T = today.AddDays noDays
let h (date: C): U = date.ToShortDateString ()
// [/snippet]
// The actual composition of the three functions
// [snippet:Function composition]
let fghExplicit = fun x -> h (g (f (x)))

let compose (fun1, fun2) = fun arg -> fun2 (fun1 (arg))

let fghOne = compose(compose(f,g),h)
let fghTwo = compose(f,compose(g,h))
// [/snippet]
// Check the output of each variation
// [snippet:The results]
let fghExplicitResult = fghExplicit myInputParam
let fghOneResult = fghOne myInputParam
let fghTwoResult = fghTwo myInputParam
// [/snippet]
