#r "Samples.Csv.dll"
open Samples.Charting.DojoChart

let urlFor ticker (startDate:System.DateTime) (endDate:System.DateTime) = 
    sprintf "http://ichart.finance.yahoo.com/table.csv?s=%s&a=%i&b=%i&c=%i&d=%i&e=%i&f=%i" 
        ticker (startDate.Month - 1) startDate.Day startDate.Year 
        (endDate.Month - 1) endDate.Day endDate.Year

type StockData = Samples.Csv.CsvFile<"http://ichart.finance.yahoo.com/table.csv?s=MSFT&a=0&b=1&c=2011&d=11&e=31&f=2011", InferTypes=true, InferRows=5>
let stockData ticker startDate endDate = new StockData(System.Uri(urlFor ticker startDate endDate))

// NOKIA in USD
let stocks = [|"NOK"|] |> Seq.map(fun symbol ->
    let data = stockData symbol (new System.DateTime(2014,1,1)) System.DateTime.Now
    let closes =
        data.Data
        |> Seq.toList
        |> List.rev   // reverse data since it shows up most recent first
        |> List.mapi (fun i s -> i, s.``Adj Close``)
    symbol, closes)
stocks |> Seq.map (fun (symbol, closes) -> Chart.Line(closes, Name=symbol).WithLegend()) |> Chart.Combine