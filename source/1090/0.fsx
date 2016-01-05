open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra.Double

let m1 = matrix [ [1.0; 2.0]; [3.0;4.0] ]
let m2 = m1 |> Matrix.map (fun v -> v * v)