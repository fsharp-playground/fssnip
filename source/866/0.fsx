module Main =
 
    let compilerBug =
        let randB = System.DateTime.Now
        Async.Parallel [ async { return randB } ]
        |> Async.RunSynchronously
 
    [<EntryPoint>]
    let main _ =
        1