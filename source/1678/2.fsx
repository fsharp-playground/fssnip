// [snippet:Async.TryFinally Implementation]
type Async with
    static member TryFinally(body : Async<'T>, finallyF : Async<unit>) = async {
        let! ct = Async.CancellationToken
        return! Async.FromContinuations(fun (sc,ec,cc) ->
            let sc' (t : 'T) = Async.StartWithContinuations(finallyF, (fun () -> sc t), ec, cc, ct)
            let ec' (e : exn) = Async.StartWithContinuations(finallyF, (fun () -> ec e), ec, cc, ct)
            Async.StartWithContinuations(body, sc', ec', cc, ct))
    }
// [/snippet]

// [snippet:Simple Examples]
let test fail wf = 
    Async.TryFinally(wf, async { if fail then failwith "boom" else printfn "42"}) 
    |> Async.RunSynchronously

test false (async { return 42}) // side effect
test true (async { return 42}) // "boom"
test false (async { return do failwith "kaboom" }) // side effect & "kaboom"
test true (async { return do failwith "kaboom" }) // "boom"
// [/snippet]

// [snippet:Application: IAsyncDisposable]
type IAsyncDisposable =
    abstract Dispose : unit -> Async<unit>

type AsyncBuilder with
    member __.Using<'T, 'U when 'T :> IAsyncDisposable>(value : 'T, bindF : 'T -> Async<'U>) : Async<'U> =
        Async.TryFinally(async { return! bindF value }, async { return! value.Dispose() })
// [/snippet]

// [snippet:IAsyncDisposable Example]

let mkDummyDisposable () = 
    { new IAsyncDisposable with 
        member __.Dispose () = async { printfn "Disposed" }}

async {
    use d = mkDummyDisposable ()
    return d.GetHashCode()
} |> Async.RunSynchronously

async {
    use d = mkDummyDisposable ()
    return null.GetHashCode()
} |> Async.RunSynchronously
// [/snippet]