// Open Math.NET namespaces (you need MathNet.Numerics package)
open MathNet.Numerics
open MathNet.Numerics.FSharp
open MathNet.Numerics.LinearAlgebra.Double

// Define type extension ofr the generic vector type 
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

// Example: Get some slices from a vector
let v = vector [ 1.0; 2.0; 3.0 ]
v.[0 .. 1] // elements [1.0; 2.0]
v.[1 ..]   // elements [2.0; 3.0]
v.[.. 1]   // elements [1.0; 2.0]