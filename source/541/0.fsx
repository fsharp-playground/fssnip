open System
open System.Collections.Generic
open System.IO

open ThoughtWorks.CruiseControl.Remote

type Result = { Name : string; Count : Int32; Average : float; StdDev : float; Min : float; Max : float; }
              member x.Print() = printfn "%s,%d,%.2f,%.2f,%.2f,%.6f" x.Name x.Count x.Min x.Max x.Average x.StdDev

let ReadLines (fileName : string) = seq {
    use reader = new StreamReader(fileName)
    while not reader.EndOfStream do yield reader.ReadLine() }

let ComputeResults name values =
    let count = values |> Seq.length
    let average = values |> Seq.average    
    let variance = (values |> Seq.map (fun n -> (n - average) * (n - average)) |> Seq.sum) / float count
    let stdDev = Math.Sqrt(variance)
    { Name = name; Count = count; Average = average; StdDev = stdDev; Min = Seq.min values; Max = Seq.max values }
            
let GetStatistics serverName projectName =
    let fileName = String.Format(@"\\{0}\ccnet\{1}\ArtifactDirectory\statistics.csv", serverName, projectName)
    ReadLines(fileName)
    |> Seq.map (fun s -> s.Split(','))      
    |> Seq.map (fun v -> TimeSpan.TryParse(v.[1]))
    |> Seq.filter (fun (r, span) -> r && span <> TimeSpan.Zero)
    |> Seq.map (fun (r, span) -> span.TotalMinutes)
    |> ComputeResults projectName

let client = new CruiseServerHttpClient("http://cruisecontrol")  

printfn "Project,Count,Minimum,Maximum,Average,StdDev"
client.GetProjectStatus()
|> Seq.map (fun (p : ProjectStatus) -> GetStatistics p.ServerName p.Name)
|> Seq.sortBy (fun result -> result.Name)
|> Seq.iter (fun result -> result.Print())