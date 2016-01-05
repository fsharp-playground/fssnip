// [snippet:Vector and matrix extensions]
// Open Math.NET namespaces (you need MathNet.Numerics package)
open MathNet.Numerics
open MathNet.Numerics.FSharp
open MathNet.Numerics.LinearAlgebra.Double

// Define type extension for the generic vector type 
// (Here we need to repeat all constraints, so it is a bit ugly)
type MathNet.Numerics.LinearAlgebra.Generic.
    Vector<'T when 'T : struct and 'T : (new : unit -> 'T) 
               and 'T :> System.IEquatable<'T> and 'T :> System.IFormattable 
               and 'T :> System.ValueType> with
  /// Implements slicing of vector - both arguments are option types
  member x.GetSlice(start, finish) = 
    let start = defaultArg start 0
    let finish = defaultArg finish (x.Count - 1)
    x.SubVector(start, finish - start + 1)

// Define type extension for the generic matrix type
type MathNet.Numerics.LinearAlgebra.Generic.
    Matrix<'T when 'T : struct and 'T : (new : unit -> 'T) 
               and 'T :> System.IEquatable<'T> and 'T :> System.IFormattable 
               and 'T :> System.ValueType> with
  // Implement slicing for matrices (using rows & columns)
  member x.GetSlice(rstart, rfinish, cstart, cfinish) = 
    let cstart = defaultArg cstart 0
    let rstart = defaultArg rstart 0
    let cfinish = defaultArg cfinish (x.ColumnCount - 1)
    let rfinish = defaultArg rfinish (x.RowCount - 1)
    x.SubMatrix(rstart, rfinish - rstart + 1, cstart, cfinish - cstart + 1)
// [/snippet]

// [snippet:Examples of slicing]
// Get some slices from a vector
let v = vector [ 1.0; 2.0; 3.0 ]
v.[0 .. 1] // elements [1.0; 2.0]
v.[1 ..]   // elements [2.0; 3.0]
v.[.. 1]   // elements [1.0; 2.0]

// Example: Get some slices from a matrix
let m = matrix [ [ 1.0; 2.0; 3.0 ]
                 [ 4.0; 5.0; 6.0 ] ]

m.[0 .. 1, 0 .. 1] // get first square 2x2
m.[0 .. 1, 2 ..]   // get the last column
m.[1 .., 0 .. 2]   // get the last row
// [/snippet]