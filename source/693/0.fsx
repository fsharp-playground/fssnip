module Retry =
    open System.Threading
    open System

    type RetryParams = {
        maxRetries : int; waitBetweenRetries : int
        }

    let defaultRetryParams = {maxRetries = 3; waitBetweenRetries = 1000}

    type RetryMonad<'a> = RetryParams -> 'a
    let rm<'a> (f : RetryParams -> 'a) : RetryMonad<'a> = f

    let internal retryFunc<'a> (f : RetryMonad<'a>) =
        rm (fun retryParams -> 
            let rec execWithRetry f i e =
                match i with
                | n when n = retryParams.maxRetries -> raise e
                | _ -> 
                    try
                        f retryParams
                    with 
                    | e -> Thread.Sleep(retryParams.waitBetweenRetries); execWithRetry f (i + 1) e
            execWithRetry f 0 (Exception())
            ) 

    
    type RetryBuilder() =
        
        member this.Bind (p : RetryMonad<'a>, f : 'a -> RetryMonad<'b>)  =
            rm (fun retryParams -> 
                let value = retryFunc p retryParams
                f value retryParams                
            )

        member this.Return (x : 'a) = fun defaultRetryParams -> x

        member this.Run(m : RetryMonad<'a>) = m

        member this.Delay(f : unit -> RetryMonad<'a>) = f ()

    let retry = RetryBuilder()

//Examples
let test() =
    
    let fn1 (x:float) (y:float) = rm (fun rp -> x * y)
    let fn2 (x:float) (y:float) = rm (fun rp -> if y = 0. then raise (invalidArg "y" "cannot be 0") else x / y)

    try
        let x = 
            (retry {
                let! a = fn1 7. 5.
                let! b = fn1 a 10.
                return b
            }) defaultRetryParams 

        printfn "first retry: %f" x

        let retryParams = {maxRetries = 5; waitBetweenRetries = 100}

        let ym = 
            retry {
                let! a = fn1 7. 5.
                let! b = fn1 a a
                let! c = fn2 b 0. //division by 0.
                return c
            }

        let y = ym retryParams
        0
    with
        e -> Console.WriteLine(e.Message); 1
 