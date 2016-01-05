open Deedle

// Given a list of dictionaries
let d1 = dict [ "A", 1.0; "B", 3.0 ]
let d2 = dict [ "A", 2.0; "C", 10.0 ]
let ds = [ d1; d2 ]

// Create a frame with a single column named "It"
// that contains the dictionaries as values
let f = frame [ "It" => Series.ofValues ds ]

// Expand all dictionaries into multiple columns
// The resulting frame has 3 columns named
// It.A, It.B and It.C
f |> Frame.expandAllCols 1

