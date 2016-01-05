module WorleyNoise
open System
open System.Collections.Generic

let worleyNoise width height pointCount seed (distanceFn : int * int -> int * int -> int) combinationFn =
    let prng = new Random(seed)
    let points = Array.init pointCount (fun _ -> prng.Next(width), prng.Next(height))
    Array2D.init width height (fun x y -> points |> Array.map (distanceFn (x, y)) |> Array.sort |> combinationFn)