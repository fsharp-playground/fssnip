open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.LinearAlgebra.Double

let linearModel (theta_vec:DenseMatrix) (data_mat:DenseMatrix) =
    let dataWithoutLabel = (DenseMatrix.init data_mat.RowCount 1 (fun i j -> 1.0)).Append(DenseMatrix.init data_mat.RowCount (data_mat.ColumnCount-1) (fun i j -> data_mat.[i,j]))
    DenseMatrix.OfMatrix(dataWithoutLabel * theta_vec)

//Define Cost Function
let j (model:DenseMatrix) (theta_vec:DenseMatrix) (data_mat:DenseMatrix) = 
    let label = DenseMatrix.init data_mat.RowCount 1 (fun i j -> data_mat.[i,(data_mat.ColumnCount-j)])
    let m : float = 50.0   //Size of data
    (1.0 /(2.0 * m) * Matrix.sum((model - label) |> Matrix.map(fun value -> value ** 2.0)))

let cost:float = j linearModel denseTheta denseData