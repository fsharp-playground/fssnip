// [snippet:Implementation of the DSL]
/// A type that represents a function that test
/// whether an array contains some pattern at a 
/// specified location. It gets the location to 
/// test & the array as arguments and returns bool.
type ShapeDetector = SD of (int -> int -> int[,] -> bool)

/// A primitive that tests whether the value at the
/// current location contains a value 'v'
let equals v = SD (fun x y arr -> arr.[x,y] = v)

/// A combinator that takes 'ShapeDetector' and 
/// creates a new one that returns 'def' when 
/// accessing outside of the array bounds
let border def (SD f) = SD (fun x y arr -> 
  if x < 0 || y < 0 || x >= arr.GetLength(0) || y >= arr.GetLength(1) 
  then def else f x y arr)

/// A combinator that calls a given ShapeDetector
/// at a location specified by offset dx, dy
let around (dx, dy) (SD f) = SD (fun x y arr ->
  f (x + dx) (y + dy) arr)

/// A combinator that takes a ShapeDetector and
/// builds a new one, which is rotated by 90 degrees
let rotate (SD f) = SD (fun x y arr ->
  f -y x arr)

/// Creates a shape detector that succeeds only
/// when both of the arguments succeed.
let (<&>) (SD f1) (SD f2) = SD (fun x y arr ->
  f1 x y arr && f2 x y arr)

/// Creates a shape detector that succeeds 
/// when either of the arguments succeed.
let (<|>) (SD f1) (SD f2) = SD (fun x y arr ->
  f1 x y arr || f2 x y arr)
// [/snippet]

// [snippet: Sample pattern detector]
// We want to detect patterns that look like this
// (with any rotation in any place of an array):
// 
//   X - -
//   - X X
//

// Create a detector that tests if a location
// contains 1 and returns 'false' when out of range
let one = border false (equals 1)

// A shape detector for your pattern
let pattern = 
  around (0, 0) one <&> around (1, 0) one <&> 
  around (-1, 1) one

// Test pattern with any rotation: Combine 
// 4 possible rotations with logical or.
let any = 
  pattern <|> rotate pattern <|> 
  rotate (rotate pattern) <|> 
  rotate (rotate (rotate pattern))
// [/snippet]

// [snippet: Run the pattern detector on a sample input]
// Create a 2D array as a sample input
let inp = 
  array2D [ [ 0; 0; 1 ]
            [ 0; 1; 0 ]
            [ 0; 1; 0 ] ]

// Get the underlying function and run it
// for all possible indices in the array
let (SD f) = any
for x in 0 .. 2 do
  for y in 0 .. 2 do
    printfn "%A %A" (x, y) (f x y inp)
// [/snippet]