open System.Diagnostics



let main =
  let w = Stopwatch.StartNew()
  let rec f (lastTime:int64) (accum:int64) : unit =
    System.Threading.Thread.Sleep 1
    let m = w.ElapsedMilliseconds
    match accum + m - lastTime with
    | elapsedTime when elapsedTime > 1000L ->
        printfn "TICK"
        f m (elapsedTime-1000L)
    | _ as elapsedTime -> f m elapsedTime


  f (w.ElapsedMilliseconds) (int64 0)