// Script example (4 of 3) for Monad blog post
// at http://rodhern.wordpress.com/2014/02/

// ---- ---- ---- ---- ---- ----
// -- Pipe functionality
// ---- ---- ---- ---- ---- ----
open System

/// Today's date and time for use in examples below.
let today = DateTime.Now

/// The test parameter I will use is "2" as in "two weeks from now".
let myInputParam = 2

// Types used below
type A = int       
type B = float     
type C = DateTime  
type D = string    

// [snippet:Functions (first example repeated)]
let f (noWeeks: A): B = float (7 * noWeeks)
let g (noDays: B): C = today.AddDays noDays
let h (date: C): D = date.ToShortDateString ()
// [/snippet]
// [snippet:The pipe operator(s)]
/// Plain old pipe operator
let pipe1 x f = f x

/// Special purpose pipe operator
let pipe2 (x, s) f =
    let fx= f x
    let msg = sprintf "fun(%s)=%A" s fx
    fx, msg
// [/snippet]
// [snippet:The results]
// My input parameters
let x0 = myInputParam
let msg0 = sprintf "%A" x0

/// Example result (plain)
let result1 =
    pipe1 (pipe1 (pipe1 x0 f) g) h

/// Example result (as tuple)
let result2, msg2 =
    pipe2 (pipe2 (pipe2 (x0, msg0) f) g) h
// [/snippet]
