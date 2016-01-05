open System.IO
let nullOut = new StreamWriter(Stream.Null) :> TextWriter

// this has the same type as printf, but it doesn't print anything
let fakePrintf fmt = fprintf nullOut fmt

// set the verbosity
let mutable verboseLevel = 3

let debug n =
    if n < verboseLevel then printfn
    else fakePrintf

debug 2 "test %d" 42 // is displayed
debug 3 "test %d" 42 // not displayed