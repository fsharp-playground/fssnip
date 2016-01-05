// Reference type provider and FSharpChart
#r @"WorldBank.TypeProvider.dll"
#load @"FSharpChart.fsx"

open Samples.Charting
open System.Windows.Forms.DataVisualization.Charting
open System.Drawing

let dashGrid = Grid(LineColor = Color.Gainsboro, LineDashStyle = ChartDashStyle.Dash)

/// Draws line chart from year-value pairs using specified label & color
let lineChart (data:seq<int * float>) name color =
  ( FSharpChart.Line(Array.ofSeq data, Name=name)
    |> FSharpChart.WithSeries.Style(BorderWidth = 2, Color = color) ) 
  :> ChartTypes.GenericChart
    
/// Calculate average university enrollment for EU
let avg =
    [ for c in WorldBank.Regions.``European Union`` do
        yield! c.``School enrollment, tertiary (% gross)`` ]
    |> Seq.groupBy fst
    |> Seq.map (fun (y, v) -> y, Seq.averageBy snd v)
    |> Array.ofSeq
    |> Array.sortBy fst

// Generate nice line chart combining CZ and EU enrollment
FSharpChart.Combine
  [ yield lineChart avg "EU" Color.Blue
    let cz = WorldBank.Countries.``Czech Republic``
    yield lineChart cz.``School enrollment, tertiary (% gross)`` "CZ" Color.DarkRed ]
|> FSharpChart.WithLegend(Docking = Docking.Left)
|> FSharpChart.WithArea.AxisY(MajorGrid = dashGrid) 
|> FSharpChart.WithArea.AxisX(MajorGrid = dashGrid)