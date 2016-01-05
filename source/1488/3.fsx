let retry maxRetries before f =
    let rec loop retriesRemaining =
        try
            f ()
        with _ when retriesRemaining > 0 ->
            before ()
            loop (retriesRemaining-1)
    loop maxRetries

// Example

let act () : unit = printfn "trying"; invalidOp "Problem"
act |> retry 2 (fun () ->  printfn "retrying";  System.Threading.Thread.Sleep 50) 

(*Yields:

trying
retrying
trying
retrying
trying
System.InvalidOperationException: Problem *)