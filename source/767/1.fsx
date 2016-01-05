open System
open System.Net
open System.Windows
open System.Windows.Shapes
open System.Windows.Media
open System.Windows.Controls
open System.Threading
open Microsoft.TryFSharp

// ------------------------------------------------------------------
// Downloading & visualizing stock prices from Yahoo


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
      if dt > from then yield (* dt, *) op } 
  return data |> Array.ofSeq |> Array.rev |> Seq.ofArray }

// Synchronous wrapper
type Yahoo = 
  static member GetPrices(stock, ?from) = 
    let from = defaultArg from DateTime.MinValue
    downloadPricesAsync from stock |> Async.RunSynchronously

/// Create a line geometry from a sequence of float values
let createGeometry (min, max) width height data =
  let offsetX, offsetY = 20.0, 20.0
  
  let rec trimData data = 
    if Seq.length data > int width / 2 then
      data |> Seq.mapi (fun i v -> i%2=0, v) 
           |> Seq.filter fst |> Seq.map snd |> trimData
    else data
  let data = trimData data
  let scale v = height - ((v - min) / (max - min) * height)
  let data = data |> Seq.map scale |> Seq.pairwise

  let geometry = new GeometryGroup()
  let step = width / float (Seq.length data)
  for i, (prev, next) in Seq.zip [ 0 .. Seq.length data ] data do
    let f = Point(offsetX + float i * step, prev + offsetY)
    let t = Point(offsetX + float (i + 1) * step, next + offsetY)
    let line = LineGeometry(StartPoint = f, EndPoint = t)
    geometry.Children.Add(line)

  geometry


/// Create line chart - returns a function that can be
/// used to set the data of the line chart
let createLineChart color width height range data =
  let path = new Path(Stroke = new SolidColorBrush(color), StrokeThickness = 2.0)
  App.Console.Canvas.Children.Add(path)
  path.Data <- createGeometry range width height data


/// Runs an F# asynchronous workflow on the GUI thread in TryFSharp
let runUserInterface work = 
  let tok = new CancellationTokenSource()
  App.Dispatch (fun() -> 
    Async.StartImmediate(work, tok.Token)
    App.Console.CanvasPosition <- CanvasPosition.Right) |> ignore
  tok

/// Mutable variable (yeah!) to count number of charts
let plotCount = ref 0
let colors = 
  [| 136,0,21; 34,177,76; 237,28,36
     0,162,232; 255,127,40; 63,72,204
     255,242,0; 163,73,163; 16, 16, 16 |]

type Plot = 
  /// Plot the specified data
  static member Line(data, ?name, ?range) =
    let range = defaultArg range (Seq.min data, Seq.max data)
    let name = defaultArg name (sprintf "Plot %d" plotCount.Value)
    let addControl ctl x y =
      Canvas.SetTop(ctl, y)
      Canvas.SetLeft(ctl, x)
      App.Console.Canvas.Children.Add(ctl)

    /// Add legend to list & create GUI
    let addLegend name color = 
      let box = Rectangle(Width = 30.0, Height = 30.0)
      addControl box 550.0 (20.0 + float (plotCount.Value * 50))
      let block = TextBlock(FontSize = 20.0, Text = name, Width = 200.0)
      addControl block 590.0 (20.0 + float (plotCount.Value * 50))
      box.Fill <- new SolidColorBrush(color)

    App.Dispatch (fun() -> 
      App.Console.CanvasPosition <- CanvasPosition.Right
      let r, g, b = colors.[plotCount.Value % colors.Length]
      let color = Color.FromArgb(255uy, byte r, byte g, byte b)
      addLegend name color
      createLineChart color 500.0 200.0 range data
      incr plotCount )

  // Function that clears the window & resets count
  static member Clear() = 
    App.Dispatch(fun () -> 
      plotCount := 0
      App.Console.Canvas.Children.Clear() ) |> ignore

// Load snippet 'co', which contains examples that use this librar
App.Dispatch (fun () -> 
  App.Console.ClearOutput()
  App.Console.LoadFromUrl("http://fssnip.net/raw/co") )