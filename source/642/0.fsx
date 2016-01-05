open System

// Discrete Frechet Distance: 
// (Based on the 1994 algorithm from Thomas Eiter and Heikki Mannila.)

let frechet (P : array<float*float>) (Q : array<float*float>) =
    let sq (x : float) = x * x
    let min3 x y z = [x; y; z] |> List.min
    let d (a : (float*float)) (b: float*float) =
        let ab_x = abs(fst(a) - fst(b))
        let ab_y = abs(snd(a) - snd(b))
        sqrt(sq ab_x + sq ab_y)

    let p, q = Array.length P, Array.length Q
    let ca = Array2D.init p q (fun _ _ -> -1.0)

    let rec c i j =
        if ca.[i, j] > -1.0 then
            ca.[i, j]
        else
            if i = 0 && j = 0 then
                ca.[i, j] <- d (P.[0]) (Q.[0])
            elif i > 0 && j = 0 then 
                ca.[i, j] <- Math.Max((c (i-1) 0), (d P.[i] Q.[0]))
            elif i = 0 && j > 0 then 
                ca.[i, j] <- Math.Max((c 0 (j-1)), (d P.[0] Q.[j]))
            elif i > 0 && j > 0 then
                ca.[i, j] <- Math.Max(min3 (c (i-1) j) (c (i-1) (j-1)) (c i (j-1)), (d P.[i] Q.[j]))
            else
                ca.[i, j] <- nan
            ca.[i, j]
    c (p-1) (q-1)

// Use frechet as an operator:
let (-~~) a1 a2 = abs(frechet a1 a2)

// Test arrays:
let linearPositive = (seq {for x in 0. .. 1. .. 100. do yield (x, x)}) |> Array.ofSeq
let linearNegative  = (seq {for x in 0. .. 1. .. 100. do yield (x, 100.-x)}) |> Array.ofSeq
let linearPositiveOffset10 = (seq {for x in 0. .. 1. .. 100. do yield (x+10., x+10.)}) |> Array.ofSeq

let x_x2 = (seq {for x in 0. .. 1. .. 100. do yield (x, x*x)}) |> Array.ofSeq
let x_x2_plus2y = (seq {for x in 0. .. 1. .. 100. do yield (x, x*x+2.)}) |> Array.ofSeq
let x_x2_plus2x = (seq {for x in 0. .. 1. .. 100. do yield (x+2., x*x)}) |> Array.ofSeq
let x_x2_plus10xy = (seq {for x in 0. .. 1. .. 100. do yield (x+10., x*x+10.)}) |> Array.ofSeq

let x_x2_scaled_x = (seq {for x in 0. .. 1. .. 100. do yield (x/2., x*x+10.)}) |> Array.ofSeq

let circle = (seq {for x in 0. .. 0.1 .. System.Math.PI*2. do yield (Math.Sin(x), Math.Cos(x))}) |> Array.ofSeq
let circleStretched = (seq {for x in 0. .. 0.1 .. System.Math.PI*2. do yield (Math.Sin(x)*4.5, Math.Cos(x)*3.4)}) |> Array.ofSeq

// Tests:
let test1 = linearPositive -~~ linearPositive // 0.0 - identical
let test2 = linearPositive -~~ linearNegative // 100.0
let test3 = linearPositive -~~ linearPositiveOffset10 // 14.14213562

let test4 = x_x2 -~~ x_x2 // 0.0 - identical
let test5 = x_x2 -~~ x_x2_plus2y // 2.0
let test6 = x_x2 -~~ x_x2_plus2x // 2.0
let test7 = x_x2 -~~ x_x2_plus10xy // 14.14213562

let test8 = x_x2 -~~ x_x2_scaled_x // 50.99019514

let test9 = circle -~~ circleStretched // 3.4998577