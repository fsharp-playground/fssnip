namespace DouglasPeuker

open System

type Point = { X: double; Y : double}

module Reduce = 

    let private findPerpendicularDistance p p1 p2 =
        if (p1.X = p2.X) then
            Math.Abs(p.X - p1.X)
        else 
            let slope = (p2.Y - p1.Y) / (p2.X - p1.X)
            let intercept = p1.Y - (slope * p1.X)
            Math.Abs(slope * p.X - p.Y + intercept) / Math.Sqrt(Math.Pow(slope, 2.) + 1.)

    let rec Reduce epsilon (points : Point[]) =
        if points.Length < 3 || epsilon = 0. then
            points
        else
            let firstPoint = points.[0]
            let lastPoint = points.[points.Length - 1]

            let mutable index = -1
            let mutable dist = 0.0

            for i in 1..points.Length-1 do
                let cDist = findPerpendicularDistance points.[i] firstPoint lastPoint
                if (cDist > dist) then
                    dist <- cDist
                    index <- i
        
            if (dist > epsilon) then
                let l1 = points.[0..index]
                let l2 = points.[index..]
                let r1 = Reduce epsilon l1
                let r2 = Reduce epsilon l2
                Array.append (r1.[0..r1.Length-2]) r2 
            else
                [|firstPoint; lastPoint|]

module Tests =

    open FsUnit
    open NUnit.Framework
    open Reduce

    [<TestFixture>]
    type ``Given the DouglasPeuker Simplify function``() = 

        let StrToPoints (s : string) =
            s.Split([|';'|])
            |> Array.map (fun p -> let xy = p.Split([|','|])
                                   {X = Double.Parse(xy.[0]); Y = Double.Parse(xy.[1])})

        // Minimal cases:
        [<TestCase("1.0, 1.0", "1.0, 1.0", 0.5)>]
        [<TestCase("1.0, 1.0; 2.0, 2.0", "1.0, 1.0; 2.0, 2.0", 0.5)>]
        [<TestCase("1.0, 1.0; 2.0, 2.0; 3.0, 3.0", "1.0, 1.0; 3.0, 3.0", 0.5)>]

        // Effect of varying epsilon:
        [<TestCase("0.0, 2.0; 1.0, 1.0; 3.0, 0.0; 5.0, 1.0", "0.0, 2.0; 1.0, 1.0; 3.0, 0.0; 5.0, 1.0", 0.1)>]
        [<TestCase("0.0, 2.0; 1.0, 1.0; 3.0, 0.0; 5.0, 1.0", "0.0, 2.0; 3.0, 0.0; 5.0, 1.0", 0.5)>]

        // Tests with vertical segments:
        [<TestCase("10.0, 35.0; 15.0, 34.0; 15.0, 30.0; 20.0, 29.0", "10.0, 35.0; 20.0, 29.0", 10.0)>]
        [<TestCase("10.0, 35.0; 15.0, 34.0; 15.0, 30.0; 20.0, 29.0", "10.0, 35.0; 15.0, 34.0; 15.0, 30.0; 20.0, 29.0", 1.0)>]

        // Tests with horizontal segments:
        [<TestCase("10.0, 35.0; 15.0, 35.0; 16.0, 30.0; 21.0, 30.0", "10.0, 35.0; 21.0, 30.0", 10.0)>]
        [<TestCase("10.0, 35.0; 15.0, 35.0; 16.0, 30.0; 21.0, 30.0", "10.0, 35.0; 15.0, 35.0; 16.0, 30.0; 21.0, 30.0", 1.0)>]

        // Tests with vertical and horizontal segments:
        [<TestCase("10.0, 30.0; 30.0, 30.0; 30.0, 10.0; 50.0, 10.0", "10.0, 30.0; 50.0, 10.0", 15.0)>]
        // Different epsilon:
        [<TestCase("10.0, 30.0; 30.0, 30.0; 30.0, 10.0; 50.0, 10.0", "10.0, 30.0; 50.0, 10.0", 10.0)>]

        // A more complex curve:
        [<TestCase("3.5, 21.25; 7.3, 12.0; 23.2, 3.1; 37.2, 12.07; 54.6, 18.15; 62.2, 16.45; 71.5, 9.7; 101.3, 21.1", "3.5, 21.25; 23.2, 3.1; 54.6, 18.15; 71.5, 9.7; 101.3, 21.1", 5.0)>]

        member public this.``inputs are correctly simplified``(items : string, expected : string, epsilon) =
            let actual = Reduce epsilon (items |> StrToPoints)
            let expected = expected |> StrToPoints
            actual |> should equal expected