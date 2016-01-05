namespace TheGame


module Fractal =

    let fractalBrownianMotion octaves lacunarity hz0 gain gain0 (fn : float -> float -> float) =
        let rec fbm hz amp octaves x y =
            if octaves = 0 then (fn (x*hz) (y*hz))*amp
            else (fn (x*hz) (y*hz))*amp + fbm (hz*lacunarity) (amp*gain) (octaves-1) x y
        fbm hz0 gain0 octaves


module GradientNoise =

    let shuffle (rand : int -> int -> int) (data : List<int>) =
        let swap (a: _[]) x y =
            let tmp = a.[x]
            a.[x] <- a.[y]
            a.[y] <- tmp
        let shuffle a = 
            Array.iteri (fun i _ -> swap a i (rand i (Array.length a))) a
            a
        shuffle (List.toArray data)

    let dot (u : float*float) (v : float*float) =
        let (x, y) = u
        let (s, t) = v
        x*s + y*t

    let mag (u : float*float) = sqrt (dot u u)

    let normalise u =
        let (x, y) = u
        let m = mag u
        x/m, y/m
    
    let makeGradNoise (seed : int) gridSize roughness =
        // MathNet library; replace with System.Random for poor performance and quality
        let rand = System.Random() //new MathNet.Numerics.Random.MersenneTwister(seed)
        let maxPerms = 1023
        let guass () = rand.NextDouble() - rand.NextDouble()
        let grads = [| for i in 0 .. maxPerms -> normalise (guass(), guass()) |]
        let gradsx, gradsy = [| for (x, y) in grads -> x |], [| for (x, y) in grads -> y |]
        let xs = shuffle (fun x y -> rand.Next(x, y)) [0 .. maxPerms]
        let ys = shuffle (fun x y -> rand.Next(x, y)) [0 .. maxPerms]
        let rGridSize = 1.0 / gridSize
        let gradNoise (x : float) (y : float) =
            let gx = x*rGridSize
            let gy = y*rGridSize
            let igx0 = int gx
            let igy0 = int gy
            let igx1 = igx0 + 1
            let igy1 = igy0 + 1
            let fgx0 = System.Math.Floor gx
            let fgy0 = System.Math.Floor gy
            let fgx1 = System.Math.Ceiling gx
            let fgy1 = System.Math.Ceiling gy
            let ydx0 = ys.[igy0 &&& maxPerms]
            let ydx1 = ys.[igy0 &&& maxPerms]
            let ydx2 = ys.[igy1 &&& maxPerms]
            let ydx3 = ys.[igy1 &&& maxPerms]
            let xdx0 = xs.[igx0 &&& maxPerms]
            let xdx1 = xs.[igx1 &&& maxPerms]
            let xdx2 = xs.[igx0 &&& maxPerms]
            let xdx3 = xs.[igx1 &&& maxPerms]
            let idx0 = xdx0 ^^^ ydx0
            let idx1 = xdx1 ^^^ ydx1
            let idx2 = xdx2 ^^^ ydx2
            let idx3 = xdx3 ^^^ ydx3
            let g0x = gradsx.[idx0]
            let g0y = gradsy.[idx0]
            let g1x = gradsx.[idx1]
            let g1y = gradsy.[idx1]
            let g2x = gradsx.[idx2]
            let g2y = gradsy.[idx2]
            let g3x = gradsx.[idx3]
            let g3y = gradsy.[idx3]
            let d0 = (gx - fgx0)*g0x + (gy - fgy0)*g0y
            let d1 = (gx - fgx1)*g1x + (gy - fgy0)*g1y
            let d2 = (gx - fgx0)*g2x + (gy - fgy1)*g2y
            let d3 = (gx - fgx1)*g3x + (gy - fgy1)*g3y
            let remX = gx-fgx0
            let remY = gy-fgy0
            let rx = remX*remX*remX*(remX*(remX*6.0-15.0)+10.0)
            let ry = remY*remY*remY*(remY*(remY*6.0-15.0)+10.0)
            let xx = (1.0-rx)*d0 + rx*d1
            let yy = (1.0-rx)*d2 + rx*d3
            (1.0-ry)*xx + ry*yy
        Fractal.fractalBrownianMotion roughness 2.0 (1.0/gridSize) 0.65 0.65 gradNoise