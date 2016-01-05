#if INTERACTIVE
#r "WindowsBase"
#r "PresentationCore"
#else
module Heatmap
#endif
open System.Windows.Media
open System

let colorIntensity (v:float) min max (colorRange:Color seq) =
    if v < min || v > max then failwithf "value %0.3f should be between min (%0.3f) and max (%0.3f)" v min max
    let nV = if max=min then 1.f else (v - min) / (max - min) |> float32 // between 0..1
    let steps = colorRange |> Seq.length
    let step = 1.f / float32 steps
    let minV = nV - step
    let maxV = nV + step
    let (<->) x (a,b) = x>=a && x<=b //between operator
    seq {for i in 0..steps -> (float32 i) * step} 
    |> Seq.windowed 2 
    |> Seq.map(fun r -> r.[0],r.[1])
    |> Seq.map(fun (a,b) ->
        if nV <-> (a,b) then abs((a-b)/2.0f-nV)
        elif minV <-> (a,b) then step-(nV-b)
        elif maxV <-> (a,b) then step-(a-nV)
        else 0.0f)
    |> Seq.zip colorRange 
    |> Seq.map (fun (c,s) -> s,Color.FromScRgb(1.f, c.ScR*s, c.ScG*s, c.ScB*s))
    |> Seq.filter (fun (a,c) -> a>0.f)
    |> Seq.map (fun (_,c)-> c.Clamp(); c)
    |> Seq.fold (+) (Color.FromScRgb(1.f,0.f,0.f,0.f))
(*
colorIntensity 0.1 0. 1. [Colors.Blue;Colors.LightGreen;Colors.Yellow;Colors.Red]
*)