#r "System.Xml.Linq.dll"
#r "packages/FSharp.Data.2.0.8/lib/net40/FSharp.Data.dll"
open FSharp.Data

// ------------------------------------------------------------------
// PART 1: Getting stock prices
// ------------------------------------------------------------------

type Stocks = CsvProvider<"MSFT.csv", InferRows=100>
let msft = Stocks.Load("http://ichart.finance.yahoo.com/table.csv?s=MSFT")

// Look at the most recent row. Note the 'Date' property
// is of type 'DateTime' and 'Open' has a type 'decimal'
let firstRow = msft.Rows |> Seq.head
let lastDate = firstRow.Date
let lastOpen = firstRow.Open

// ------------------------------------------------------------------
// PART 2: Charting stock prices
// ------------------------------------------------------------------

#load "packages/FSharp.Charting.0.90.6/FSharp.Charting.fsx"
open System
open FSharp.Charting

// Print the prices in the HLOC format
for row in msft.Rows do
  printfn "HLOC: (%A, %A, %A, %A)" row.High row.Low row.Open row.Close

// Visualize the stock prices
let data = 
  [ for row in msft.Rows do
      if row.Date > DateTime.Now.AddDays(-60.0) then
          yield row.Date, row.Open ]

Chart.FastLine(data)
|> Chart.WithYAxis(Min = 25.0)


// DEMO: Range chart 
// TODO: Candlestick 
let recent = [DateTime.Now, 50.0, 10.0, 20.0, 30.0]

// Visualize prices using Stock/Candlestick chart
Chart.Stock(recent)
|> Chart.WithYAxis(Max = 100.0, Min = 0.0)