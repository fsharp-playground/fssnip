// [snippet:Downloading lines as asynchronous sequence]
// F# Async Extensions (implement asynchronous sequences etc.)
// Available at: https://github.com/tpetricek/FSharp.AsyncExtensions
#r "FSharp.AsyncExtensions.dll"
open System
open System.Net
open System.Text
open FSharp.IO
open FSharp.Control
open Microsoft.FSharp.Control.WebExtensions

/// Asynchronously download lines of a specified file
/// (content is decuded using ASCII encoding)
let downloadLines (url:string) = asyncSeq {
  // Create HTTP request and get response asynchronously
  let req = HttpWebRequest.Create(url)
  let! resp = req.AsyncGetResponse()
  let stream = resp.GetResponseStream()

  let str = ref ""
  // Download content in 1kB buffers 
  for buffer in stream.AsyncReadSeq(1024) do
    // Decode buffer using ASCII and add to remaining text
    str := str.Value + String(Encoding.ASCII.GetChars(buffer)) + " "

    // Yield all lines except for the (incomplete) last one
    let parts = str.Value.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
    for i in 0 .. parts.Length - 2 do
      yield parts.[i]

    // Save the unprocessed rest of text for the next iteration
    let rest = parts.[parts.Length - 1]
    str := rest.Substring(0, rest.Length - 1)

  // Yield the last line if it is not empty
  if str.Value <> "" then yield str.Value }
// [/snippet]

// [snippet:Getting stock prices from Yahoo]
// Yahoo URL with historical stock prices
let ystock = "http://ichart.finance.yahoo.com/table.csv?s="

// Download data for MSFT and skip the header line 
downloadLines (ystock + "MSFT")
|> AsyncSeq.skip 1
|> AsyncSeq.map (fun line ->
     // Split line into Open, High, Low, Close values
     let infos = line.Split(',')
     float infos.[1], float infos.[2], float infos.[3], float infos.[4])
// Take first 30 values and start printing asynchronously
|> AsyncSeq.take 30
|> AsyncSeq.iter (printfn "%A")
|> Async.Start
// [/snippet]