#load "BinPacking.fs"
#r "System.Windows.Forms.DataVisualization"

[<Measure>] type m //minutes
[<Measure>] type ft //feet

type Machine = {Id:string; ProcessingRate:int<ft/m>}
type Task = {Id:string; Demand:int<ft>}

let rnd = System.Random()
let toRate (x:int) = x*1<ft/m>
let toFeet (x:int) = x*1<ft>

let machines = [for i in 1..5 -> {Id=sprintf "M %d" i; ProcessingRate=rnd.Next(5, 20) |> toRate}]

let tasks = [for i in 1..30 -> {Id=sprintf "J %d" i; Demand=rnd.Next(100,200) |> toFeet }]

open BinPacking

let timeHorizon = 60<m>

let bins = machines |> List.map (fun m -> {Id=m.Id; Size=m.ProcessingRate * timeHorizon |> int; Data=m})
let items = tasks |> List.map (fun j -> {Size=j.Demand |> int; Data=j})
let sortedItems = items |> List.sortBy (fun i -> -i.Size) //sort in decreasing order

let schedule,_,_ = packBestFit bins sortedItems //generate a schedule using best fit heuristics
//let schedule,_,_ = packWorstFit bins sortedItems //generate a schedule using worst fit heuristics

//vizualization

//visualization helper type
type Job = {TaskId:string; MachineId:string; Start:int; End:int; Rate:int }

//list of jobs created from the schedule
let plan =
    schedule
    |> Map.toSeq
    |> Seq.collect (fun (machine,tasks) ->
        let _,jobs =
            ((0,[]),tasks) ||> List.fold (fun (offset,acc) item -> 
                let taskMinutes = item.Data.Demand / machine.Data.ProcessingRate
                let job =
                    {
                        TaskId=item.Data.Id
                        MachineId = machine.Id
                        Start = offset
                        End = offset + (taskMinutes |> int) 
                        Rate = machine.Data.ProcessingRate |> int
                    }
                job.End,job::acc)
        jobs
       )
    |> Seq.toList

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open System.Drawing


//creates chart
let visualizePlan (plan:Job list) =

    //set up the chart window, plot area, titles, etc.
    let titleFont = new Font("Arial Black", 12.f, FontStyle.Bold)
    let chart = new Chart(Dock = DockStyle.Fill)
    chart.Titles.Add ("Job Schedule") |> fun t -> t.Font <- titleFont
    chart.BorderSkin.SkinStyle <- BorderSkinStyle.Emboss
    chart.BorderlineWidth <- 2
    chart.BorderDashStyle <- ChartDashStyle.Solid
    chart.BorderlineColor <- Color.DarkBlue
    let area = new ChartArea("Main")
    area.AxisY.Title <- "Minutes (offset)"
    area.AxisX.Title <- "Machines"
    area.AxisX.TitleFont <- titleFont
    area.AxisY.TitleFont <- titleFont
    area.BackColor <- Color.AntiqueWhite
    area.BackSecondaryColor <- Color.Yellow
    area.BorderColor <- Color.Blue
    area.BorderWidth <- 1
    area.ShadowOffset <- 5
//    area.Area3DStyle.Enable3D <- true
    chart.ChartAreas.Add(area)

    let mainForm = new Form(Visible = true, TopMost = true, Width = 700, Height = 500)
    mainForm.Controls.Add(chart)

    //get the unique list of machines in the plan
    let machines = plan |> List.map (fun  p -> p.MachineId,p.Rate) |> Seq.distinct |> Map.ofSeq

    //color each job according to machine rate
    let rates = machines |> Map.toList |> List.map(fun (_,r) -> r)
    let rMax,rMin = List.max rates |> float, List.min rates |> float
    let mColors =
        machines |> Map.map (fun m rate -> 
            let greenness = if rMax=rMin then 0 else 190 - int( ((float rate - rMin) / (rMax - rMin)) * 190.)
            Color.FromArgb(210,255, greenness, 50))

    let series = new Series("Jobs", ChartType = SeriesChartType.RangeBar, BorderWidth = 4)
    chart.Series.Add(series)

    //using the list of machines, fill in the data series points
    machines 
    |> Map.toSeq
    |> Seq.iteri (fun i (mchn, _) -> 
        plan 
        |> List.filter (fun p -> p.MachineId = mchn) 
        |> List.iter (fun job -> 

            let idx = series.Points.AddXY(i, job.Start, job.End)

            //set other visualization attributes of the point just added
            let pt = series.Points.[idx]
            pt.Color <- mColors.[mchn]
            pt.AxisLabel <- job.MachineId
            pt.Label <- job.TaskId
            pt.ToolTip <- sprintf "%s, Time: %dm, M.Rate=%d" job.TaskId (job.End-job.Start) job.Rate
            pt.LabelForeColor <- Color.Gray
            pt.LabelBackColor <- Color.AntiqueWhite
            pt.BorderWidth <- 2
            pt.BorderColor <- Color.AntiqueWhite
            )
        )
    mainForm
    
visualizePlan plan