open System

let longComputation x =  
  let startTime = DateTime.Now.Ticks
  async {
    printfn "item of %d starts is calculation at %s" x (TimeSpan(DateTime.Now.Ticks - startTime).ToString("G"))
    let sleeptime = x * 100
    do! Async.Sleep sleeptime
    printfn "item of %d processed at %s" x (TimeSpan(DateTime.Now.Ticks - startTime).ToString("G"))
    return x + 1
  }

let computation = [1 .. 5] |> List.map (fun item -> longComputation item) |> Async.Parallel
printfn "Result of computations is %d" (computation |> Async.RunSynchronously |> Array.sum)       