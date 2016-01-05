open System
open System.Net

/// Asynchronously downloads stock prices from Yahoo
/// (uses a proxy to enable cross-domain downloads)
let downloadPricesAsync from stock = async {
  // Download price from Yahoo
  let wc = new WebClient()
  let url = "http://ichart.finance.yahoo.com/table.csv?s=" + stock  
  let proxy = "http://tomasp.net/tryjoinads/proxy.aspx?url=" + url
  let! html = wc.AsyncDownloadString(Uri(proxy)) 
  let lines = html.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)

  // Return sequence that reads the prices
  let data = seq { 
    for line in lines |> Seq.skip 1 do
      let infos = (line:string).Split(',')
      let dt = DateTime.Parse(infos.[0])
      let op = float infos.[1] 
      if dt > from then yield dt, op } 
  return data |> Array.ofSeq |> Array.rev |> Seq.ofArray }

// ------------------------------------------------------------------
// Helpers & wrappers

module Seq = 
  let rev data = data |> List.ofSeq |> List.rev |> Seq.ofList

// Synchronous wrapper
type Yahoo = 
  static member GetPrices(stock, ?from) = 
    let from = defaultArg from DateTime.MinValue
    downloadPricesAsync from stock |> Async.RunSynchronously
  static member GetPriceTable(stock, ?from) = 
    let from = defaultArg from DateTime.MinValue
    downloadPricesAsync from stock |> Async.RunSynchronously |> dict
  static member GetPriceTable(?from) =
    [ for stock in [ "MSFT"; "AAPL"; "FB"; "YHOO" ] do 
        for dt, v in Yahoo.GetPrices(stock, ?from=from) do
          yield (stock, dt), v ] |> dict

// Load snippet 'co', which contains examples that use this librar
App.Dispatch (fun () -> 
  App.Console.ClearOutput()
  App.Console.LoadFromUrl("http://fssnip.net/raw/cq") )