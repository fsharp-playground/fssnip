// [snippet:Link to blog post]
// Script example (2 of 3) for Monad blog post
// at http://rodhern.wordpress.com/2014/02/
// [/snippet]
open System

/// Today's date and time for use in examples below.
let today = DateTime.Now

/// The test parameter I will use is "2" as in "two weeks from now".
let myInputParam = 2


// ---- ---- ----
// -- The difficult example of function composition
// ---- ---- ----

// The preparation (f: A -> S, g: B -> T, g: C -> U)
// [snippet:Types]
type A = int
type S = int
type B = float
type T = DateTime
type C = int * int * int
type U = string
// [/snippet]
// [snippet:Functions]
let f (noWeeks: A): S = 7 * noWeeks
let g (noDays: B): T = today.AddDays noDays
let h ((yyyy, mm, dd): C): U = sprintf "%04d-%02d-%02d" yyyy mm dd
// [/snippet]
// The actual composition of the three functions
// [snippet:Function composition]
let composeSB: S * (B -> 'T) -> 'T = fun (x, fn) -> fn (float x)
let composeTC: T * (C -> 'U) -> 'U = fun (x, fn) -> fn (x.Year, x.Month, x.Day)

let fghOne = fun x -> composeTC(composeSB(f x, g), h)
let fghTwo = fun x -> composeSB(f x, (fun y -> composeTC(g y, h)))
// [/snippet]
// Check the output of each variation
// [snippet:The results]
let fghOneResult = fghOne myInputParam
let fghTwoResult = fghTwo myInputParam
// [/snippet]
