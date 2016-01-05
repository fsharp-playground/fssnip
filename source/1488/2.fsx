let retry maxRetries before f =
    let rec loop retriesRemaining =
        try
            f ()
        with _ when retriesRemaining > 0 ->
            before ()
            loop (retriesRemaining-1)
    loop maxRetries
