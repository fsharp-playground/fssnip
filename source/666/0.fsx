namespace modelling.shared

[<AutoOpen>]
module Plotting =

    open System.Drawing
    open System.Windows.Forms
    open System.Windows.Forms.DataVisualization.Charting
    open Microsoft.FSharp.Reflection
    open System.Reflection
    open MSDN.FSharp.Charting
    open MSDN.FSharp.Charting.ChartStyleExtensions
    open System

    let (|FindChartOfType|_|) numGenericArgs chartType =
        let mi = 
            (typeof<FSharpChart>.GetMethods() 
            |> Array.filter(
                fun v -> 
                    v.Name = chartType && v.GetParameters().Length = 1 && v.GetGenericArguments().Length = numGenericArgs)).[0]
        match mi with
        | x when x = Unchecked.defaultof<MethodInfo> -> None
        |_ -> Some mi

    let createForm (chart : ChartTypes.CombinedChart) =
            let chartForm = new ChartForm<ChartData.DataSourceCombined>(chart)
            chartForm.Text <- "Chart"
            chartForm.ClientSize <- new Size(600, 600)
            Application.EnableVisualStyles()
            Application.Run(chartForm :> Form)

    let findAndCreateChart chartType (genericArgs : Type []) name y =
        let mi =
            match chartType with
            | FindChartOfType genericArgs.Length mi -> mi
            | _ -> invalidArg "chartType " ("Chart of type " + chartType + " not found")

        let chart = mi.GetGenericMethodDefinition().MakeGenericMethod(genericArgs).Invoke(null, [|y|]) :?> ChartTypes.GenericChart
        chart.Name <- name
        chart

    type Charting () =  

        static member private createChartOfType ((chartType : string), name, (y : seq<#IConvertible * #IConvertible>))  =
            let innerTps = FSharpType.GetTupleElements(y.GetType().GetGenericArguments().[0])
            findAndCreateChart chartType innerTps name y

        static member private createChartOfType ((chartType : string), name, (y : seq<#IConvertible>))  =
            let innerTp = y.GetType().GetGenericArguments().[0]
            findAndCreateChart chartType [|innerTp|] name y
  
        static member Plot 
            (
            chartType : string,
            plotY : #seq<#IConvertible> seq, 
            ? plotX : #seq<#IConvertible>,
            ? seriesNames : string seq,
            ? title : string,
            ? xTitle : string,
            ? yTitle : string, 
            ? xLimits : float * float, 
            ? yLimits : float * float, 
            ? margin : float32 * float32 * float32 * float32) =

            let marg = defaultArg margin (4.0f, 12.0f, 4.0f, 4.0f)
            let chartTitle = defaultArg title "Chart"
            let xTitle = defaultArg xTitle String.Empty
            let yTitle = defaultArg yTitle String.Empty
            let chartNames = defaultArg seriesNames (plotY |> Seq.mapi(fun i v -> "Series " + i.ToString()))
            if (chartNames |> Seq.length) <> (plotY |> Seq.length) then invalidArg "names" "not of the right length"
            
            // zip up the relevant information together
            // x-values go with every y-series values in a tuple
            // series names gets zipped with every sequence of (x, y) tuples: (name, (x, y))
            let mutable chart = 
                match plotX with
                |Some plotX ->
                    let plot = plotY |> Seq.map(fun s -> List.zip plotX s) |> Seq.zip chartNames
                    FSharpChart.Combine ([for p in plot -> Charting.createChartOfType (chartType, (fst p), (snd p))])

                | None -> 
                    let plot = plotY |> Seq.zip chartNames
                    FSharpChart.Combine ([for p in plot -> Charting.createChartOfType (chartType, (fst p), (snd p))])

            
            //add x and y limits
            chart <- 
                match xLimits with
                | Some (xMin, xMax) -> FSharpChart.WithArea.AxisX(Minimum = xMin, Maximum= xMax, MajorGrid = Grid(LineColor = Color.LightGray)) chart
                | None -> FSharpChart.WithArea.AxisX(MajorGrid = Grid(LineColor = Color.LightGray)) chart

            chart <-
                match yLimits with
                | Some (yMin, yMax) -> FSharpChart.WithArea.AxisY(Minimum = yMin, Maximum= yMax, MajorGrid = Grid(LineColor = Color.LightGray)) chart
                | None -> FSharpChart.WithArea.AxisY(MajorGrid = Grid(LineColor = Color.LightGray)) chart
                //... and margin
                |> FSharpChart.WithMargin marg

            //set the titles
            chart.Area.AxisX.Title <- xTitle
            chart.Area.AxisY.Title <- yTitle

            //add legend
            chart <- 
                FSharpChart.WithLegend(InsideArea = false, Alignment = StringAlignment.Center, Docking = Docking.Top) chart

            //add title
            chart.Title <- StyleHelper.Title(chartTitle, FontSize = 10.0f, FontStyle = FontStyle.Bold)

            //create the form
            createForm chart