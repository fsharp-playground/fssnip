// [snippet:Setup]
// Install FSharp.Charting package from NuGet
// and then reference the charting library for
// easy visualization from F# Interactive
#load @"packages\FSharp.Charting.0.90.5\FSharp.Charting.fsx"
open FSharp.Charting
// [/snippet]

// [snippet:Infinite sums]
/// Generates partial sums of the reciprocal series
/// First generate series, then use scan for partial sums
let eulerSums =
  Seq.initInfinite (fun i -> 1.0 / float (i + 1) ** 2.0) 
  |> Seq.scan (+) 0.0

/// Generate partial sums of grandi series (even easier!)
let grandiSums = 
  Seq.initInfinite (fun i -> -1.0 ** float i)
  |> Seq.scan (+) 0.0

/// Generate partial sums of the grandiSums series
/// using the cesaro summation (simply take the sums
/// and divide by the element index in mapi)
/// (We skip the initial zero elements produced by scan) 
let grandiCesaro = 
  grandiSums
  |> Seq.skip 1
  |> Seq.scan (+) 0.0
  |> Seq.mapi (fun i partial -> partial / float i )
  |> Seq.skip 1
// [/snippet]

// [snippet:Visualization]
// Take 100 (or whatever number of elements)
// and pass the series to Chart.Line to see a chart  
eulerSums
|> Seq.take 100
|> Chart.Line

// Same for the other visualizations!
grandiSums   |> Seq.take 100 |> Chart.Line
grandiCesaro |> Seq.take 100 |> Chart.Line
// [/snippet]