open System.Windows

[<System.STAThreadAttribute>]
do
  let timer = System.Diagnostics.Stopwatch.StartNew()
  let app = Application()
  let rec f =
    new System.EventHandler(fun (_: obj) (_: System.EventArgs) ->
      printfn "First Rendering event at %fs" timer.Elapsed.TotalSeconds
      Media.CompositionTarget.Rendering.RemoveHandler f)
  Media.CompositionTarget.Rendering.AddHandler f
  app.Run(Window()) |> ignore