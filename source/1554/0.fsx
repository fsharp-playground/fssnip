(*[omit:opens]*)
open Hopac
open Hopac.Infixes
open Hopac.Job.Infixes
open Hopac.Alt.Infixes
open System.Linq
(*[/omit]*)

let parallelMap (f: 'T -> 'R) input = 
    let rec loop input = Job.delay <| fun _ ->
        match input with 
        | [] -> Job.result []
        | x :: xs -> Job.lift f x <*> loop xs |>> fun (y, ys) -> y :: ys
    run (loop input)

let sleepF _ = System.Threading.Thread.Sleep 10
let xs = [1..100]

xs |> List.map sleepF |> ignore
// Real: 00:00:01.003, CPU: 00:00:00.000, GC gen0: 0, gen1: 0, gen2: 0

xs.AsParallel().Select(sleepF).ToList() |> ignore
// Real: 00:00:00.286, CPU: 00:00:00.000, GC gen0: 0, gen1: 0, gen2: 0

xs |> parallelMap sleepF |> ignore
// Real: 00:00:00.262, CPU: 00:00:00.000, GC gen0: 0, gen1: 0, gen2: 0

let calcF x = sin (float x) / cos (float x)
let ys = [1..5000000]

ys |> List.map calcF |> ignore
// Real: 00:00:00.689, CPU: 00:00:00.858, GC gen0: 2, gen1: 0, gen2: 0

ys.AsParallel().Select(calcF).ToList() |> ignore
// Real: 00:00:01.434, CPU: 00:00:02.854, GC gen0: 1, gen1: 1, gen2: 1

ys |> parallelMap calcF |> ignore
Real: 00:00:05.453, CPU: 00:00:11.263, GC gen0: 24, gen1: 1, gen2: 0