open System
open System.Drawing
open System.IO
open System.Windows.Forms.DataVisualization.Charting
open MSDN.FSharp.Charting

/// parse date and distance info from file
let getRunData path =
    File.ReadAllLines(path)
    |> Array.map (fun line ->
        match line.Split([|' '|], StringSplitOptions.RemoveEmptyEntries) with
        | [| date; dist |] ->
            (DateTime.Parse(date) , float dist)
        | _ -> failwith "Unable to parse line")

/// plot total progress, daily distance, and goal progress
let makeRunChart (runData : (DateTime * float) array) goal =
    let lastDate = fst (runData.[runData.Length - 1])
    let endDate =   // only chart up to the end of the current month
        DateTime.MinValue.AddYears(lastDate.Year - 1).AddMonths(lastDate.Month).AddDays(-1.)
    let startDate = DateTime.MinValue.AddYears(lastDate.Year - 1)
    let dailyGoal = goal / 365.  // not worrying about leap years, I know...

    // small series representing first and last day at goal pace
    let goalPts =
        let totalDays = (endDate - startDate).TotalDays + 1.
        [(startDate, dailyGoal); (endDate, dailyGoal * totalDays)]

    // convert input list of daily distances into a list of cumulative distance
    let sumPts = 
        let sum = ref 0.
        runData
        |> Array.map (fun (date, dist) ->
            sum := !sum + dist
            (date, !sum))

    // column chart of daily runs
    let runChrt = 
        FSharpChart.Column (runData, Name = "Daily miles run")
        |> FSharpChart.WithSeries.Style (Color = System.Drawing.Color.Gray)
        |> FSharpChart.WithSeries.AxisType (YAxisType = AxisType.Secondary)

    // line chart of total progress
    let progressChrt =    
        FSharpChart.Line (sumPts, Name = "Total miles run")
        |> FSharpChart.WithSeries.Style (Color = System.Drawing.Color.Red, BorderWidth = 4)
        |> FSharpChart.WithSeries.Marker (Style = MarkerStyle.Circle, Size = 7)

    // line chart of goal progress
    let goalChrt = 
        FSharpChart.Line (goalPts, Name = "Target miles")
        |> FSharpChart.WithSeries.Style (Color = System.Drawing.Color.LightSteelBlue, BorderWidth = 4)

    // complete chart
    FSharpChart.Combine [runChrt; goalChrt; progressChrt]
    |> FSharpChart.WithArea.AxisX (Minimum = startDate.ToOADate(), Maximum = endDate.ToOADate(), MajorGrid = Grid(Enabled = false))
    |> FSharpChart.WithArea.AxisY (Title = "Total miles", TitleFont = new Font("Calibri", 11.0f), MajorGrid = Grid(LineColor = System.Drawing.Color.LightGray))
    |> FSharpChart.WithArea.AxisY2 (Title = "Daily miles", TitleFont = new Font("Calibri", 11.0f), MajorGrid = Grid(Enabled = false), Maximum = 10.)
    |> FSharpChart.WithTitle (Text = (sprintf "%.0f Miles in %d - Progress" goal lastDate.Year), Font = new Font("Impact", 14.0f))
    |> FSharpChart.WithLegend (Font = new Font("Calibri", 10.0f))   
    |> FSharpChart.WithCreate