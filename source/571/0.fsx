module WorleyNoise
open System
open System.Collections.Generic

let worleyNoise width height pointCount seed (distanceFn : int * int -> int * int -> int) combinationFn =
    let cells = Array2D.create width height 0
    let prng = new Random(seed)
    let points = Array.init pointCount (fun _ -> prng.Next(width), prng.Next(height))
    for x in 0 .. width - 1 do
        for y in 0 .. height - 1 do
            cells.[x, y] <- Array.map (distanceFn (x, y)) points |> Array.sort |> combinationFn
    cells