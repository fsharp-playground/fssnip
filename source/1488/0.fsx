let retryWithSleep (sleepSpan:System.TimeSpan) maxRetries f =
    let rec loop retriesRemaining =
        try
            f()
        with _ when retriesRemaining <> 0 ->
            System.Threading.Thread.Sleep sleepSpan
            loop <| retriesRemaining-1 
    loop maxRetries