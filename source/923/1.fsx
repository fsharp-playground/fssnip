// Completely useless exploration of the question:
// how sparse is the product of 2 sparse matrices?

#r "FSharp.PowerPack.dll"

let density (M: Matrix<float>) =
    let elements = M.NumRows * M.NumCols |> (float)
    let nonZero =
        M
        |> Matrix.map (fun e -> 
            if e = 0.0 then 0.0 else 1.0)
        |> Matrix.sum
    nonZero / elements
  
let dense = matrix [ [ 42.0; 0.0  ]; 
                     [-1.0;  123.0] ]
let sparse = matrix [ [ 0.0; 1.0 ]; 
                      [ 0.0; 0.0 ] ]

printfn "Dense matrix: %f" (density dense)
printfn "Sparse matrix: %f" (density sparse)

let rng = new System.Random()

let create n density =
    Matrix.create n n 0.0
    |> Matrix.map (fun e -> 
        if rng.NextDouble() > density 
        then 0.0 
        else 1.0)

// Run r times the product of 2 matrices
// of density d, and size n, and compute
// the average density
let simulation n d r =
    Seq.initInfinite (fun index ->
        let m1 = create n d
        let m2 = create n d
        density (m1 * m2))
    |> Seq.take r
    |> Seq.average

// Relationship between density and density
for density in 0.0 .. 0.05 .. 0.5 do
    simulation 10 density 1000
    |> printfn "Density %f -> Result is %f" density

// Relationship between size and density
for size in 5 .. 5 .. 50 do
    simulation size 0.1 1000
    |> printfn "Size %i -> Result is %f" size