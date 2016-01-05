module Retry    
    
    open System

    type RetryPolicy = Policy of (int -> exn -> TimeSpan option)

    /// retries given action based on policy
    let retry (Policy p) (f : unit -> 'T) =
        let rec aux retries =
            let result = 
                try Choice1Of2 <| f () 
                with e ->
                    match p (retries + 1) e with
                    | None -> reraise ()
                    | Some interval -> Choice2Of2 interval

            match result with
            | Choice1Of2 t -> t
            | Choice2Of2 interval ->
                do System.Threading.Thread.Sleep interval
                aux (retries + 1)

        aux 0


    // sample policies

    [<Measure>] type sec

    let ofSeconds (seconds : float<sec> option) = 
        match seconds with
        | None -> TimeSpan.Zero
        | Some secs -> TimeSpan.FromSeconds (float secs)

    type RetryPolicy with
        static member NoRetry = Policy(fun _ _ -> None)
        static member Infinite (?delay : float<sec>) = 
            Policy(fun _ _ -> Some <| ofSeconds delay)

        static member Retry(maxRetries : int, ?delay : float<sec>) =
            Policy(fun retries _ ->
                if retries > maxRetries then None
                else Some <| ofSeconds delay)

        static member ExponentialDelay(maxRetries : int, initialDelay : float<sec>) =
            Policy(fun retries _ ->
                if retries > maxRetries then None
                else
                    let delay = initialDelay * (2.0 ** float (retries - 1))
                    Some <| TimeSpan.FromSeconds(float delay))


    // examples

    let succeedAfter n =
        let count = ref 0
        fun () ->
            incr count
            printfn "%d" !count
            if !count > n then ()
            else
                failwith "not yet"


    succeedAfter 10 |> retry (RetryPolicy.Retry(5, 0.1<sec>))
    succeedAfter 10 |> retry (RetryPolicy.Retry(20, 0.1<sec>))
    succeedAfter 10 |> retry (RetryPolicy.Infinite())
    succeedAfter 5 |> retry (RetryPolicy.ExponentialDelay(10, 0.3<sec>))