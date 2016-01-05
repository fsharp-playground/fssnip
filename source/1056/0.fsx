open Fmat.Numerics
open Fmat.Numerics.MatrixFunctions
open Fmat.Numerics.BasicStat

let calcPi n =
    let x = rand [1;n]
    let y = rand [1;n]
    let d = x .* x + y .* y
    let circ = new Matrix(d .< 1.0)
    let m = sum(circ,1)
    float(m)/(float)n*4.0

let pi = calcPi 4000000