open System.Threading;

let lazyFuture (f : unit -> 'a) =
    let sub = new ReplaySubject<'a option * Exception>()
    async {
        try
            let r = f()
            sub.OnNext(Some r,null); ()
            ()
        with
        | ex -> sub.OnNext(None,ex); ()
        
    } |> Async.Start
    lazy
    (
        match sub.ToEnumerable().First() with
        | (Some r,_) -> r
        | (_,e) -> raise e
    )

let v = lazyFuture (fun () -> Thread.Sleep(3000); 10)

v.Force() |> printfn "%A"