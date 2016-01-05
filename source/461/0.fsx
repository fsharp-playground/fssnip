//Open proper namespaces

let combineLatestToArray (list : IObservable<'T> list) = 
    let s = new Subject<'T array>()
    let arr = Array.init list.Length (fun _ -> Unchecked.defaultof<'T>)
    let main = list |> List.mapi (fun i o -> o.Select(fun t -> (i,t)))
               |> Observable.Merge
    async {
        try
            let se = main.ToEnumerable() |> Seq.scan (fun ar (i,t) -> Array.set ar i t; ar) arr
            for i in se do
                s.OnNext(i |> Array.toList |> List.toArray)
            s.OnCompleted()
        with
        | :? Exception as ex -> s.OnError(ex)
    } |> Async.Start
    s :> IObservable<'T array>