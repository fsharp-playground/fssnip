open MathNet.Numerics.LinearAlgebra.Double

let gds_orig (X: Matrix) (y: Vector) (th: Vector) (alp: float) =
  let m = y.Count  
  let n = X.ColumnCount
  let mf = float m
  let oldth = th.Clone()
  for j = 0 to n - 1 do 
    printfn "Regression for feature: %i" j
    let acc = 
      [| 
        for i = 1 to m - 1 do 
          yield async { 
            return ((X.Row(i) * oldth) - y.[i]) * (X.[i,j]) 
          }     
      |] |> Async.Parallel |> Async.RunSynchronously
         |> Array.sum
         |> (fun acc -> acc * (alp / mf))
    do th.[j] <- oldth.[j] - acc 
  th

let gds_fun (X: Matrix) (y: Vector) (th: Vector) (alp: float) =
  th |> Vector.mapi (fun j v -> 
     printfn "Feature: %i" j
     let acc = 
        X |> Matrix.sumRowsBy 
              (fun i xr -> (xr * th - y.[i]) * X.[i,j])
          |> (fun acc -> acc * (alp / float y.Count))
     in v - acc)

// Tweet Sized!
let gds_golf (X: Matrix) (y: Vector) (θ: Vector) (α: float) =
  θ |> Vector.mapi (fun j v -> 
    v - (X |> Matrix.sumRowsBy (fun i xr -> 
      (xr * θ - y.[i]) * X.[i,j]) 
      |> (*) (α / float y.Count)))

            
// Tweet sized with words!
let gds_vec (X: Matrix) (y: Vector) (θ: Vector) (α: float) =
    θ - ((X * θ - y) * X * (α / float y.Count))
