open MathNet.Numerics.LinearAlgebra.Double

let gds_orig (X: Matrix) (y: Vector) (theta: Vector) (alpha: float) =
    let m = y.Count
    let n = X.ColumnCount
    let mf = float m
    let oldtheta = theta.Clone()
    for j = 0 to n - 1 do 
        printfn "Regression for feature: %i" j
        let acc = 
            [| 
                for i = 1 to m - 1 do 
                yield async { return ((X.Row(i) * oldtheta) - y.[i]) * (X.[i,j]) }     
            |] |> Async.Parallel |> Async.RunSynchronously
            |> Array.sum
            |> (fun acc -> acc * (alpha / mf))
        theta.[j] <- oldtheta.[j] - acc 
    theta

let gdstep_comb (X: Matrix) (y: Vector) (theta: Vector) (alpha: float) =
    theta 
    |> Vector.mapi (fun j v -> 
        printfn "Feature: %i" j
        let acc = 
            X 
            |> Matrix.sumRowsBy (fun i xrow -> (xrow * theta - y.[i]) * X.[i,j])
            |> (fun acc -> acc * (alpha / float y.Count))
            in v - acc)

let gdstep_golf (X: Matrix) (y: Vector) (θ: Vector) (α: float) =
    θ |> Vector.mapi (fun j v -> 
        v - (X |> Matrix.sumRowsBy (fun i xr -> 
                        (xr * θ - y.[i]) * X.[i,j]) 
                        |> (*) (α / float y.Count)))

            
let gdstep_vector (X: Matrix) (y: Vector) (θ: Vector) (α: float) =
    θ - ((X * θ - y) * X * (α / float y.Count))
