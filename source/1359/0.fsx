module AsyncEither

type AsyncEither<'a, 'b> =
    Async<Choice<'a, 'b>>

let inline bind (m : AsyncEither<'a, 'b>) (f : 'a -> AsyncEither<'c, 'b>) : AsyncEither<'c, 'b> =
    async {
        let! c = m
        match c with
        | Choice1Of2 data ->
            return! f data
        | Choice2Of2 error ->
            return Choice2Of2 error
    }

let returnM m : AsyncEither<'a, 'b> =
    async { return Choice1Of2 m }

type AsyncEitherBuilder () =
    member x.Return m = returnM m
    member x.Bind (m, f) = bind m f
    member x.ReturnFrom m = m

let asyncChoose = AsyncEitherBuilder()

// example

let remoteCall request =
    async {
        do! Async.Sleep 100                
        return Choice1Of2 request
    }

let failedCall request =
    async {
        do! Async.Sleep 100
        return Choice2Of2 (exn ("I failed! :( - " + request))
    }

let chainedSuccess message =
    asyncChoose {
        let! firstCall = remoteCall message
        let! secondCall = remoteCall (firstCall + " second")
        
        return secondCall }
    |> Async.RunSynchronously
    |> function
       | Choice1Of2 response ->
            sprintf "Yay, I worked: %s" response
       | Choice2Of2 e ->
            sprintf "Boo, a failure: %A" e

//> chainedSuccess "hello world";;
//> val it : string = "Yay, I worked: hello world second"

let withFail message =
    asyncChoose {
        let! failure = failedCall message
        let! firstCall = remoteCall failure
        let! secondCall = remoteCall (firstCall + " second")
        
        return secondCall }
    |> Async.RunSynchronously
    |> function
       | Choice1Of2 response ->
            sprintf "Yay, I worked: %s" response
       | Choice2Of2 e ->
            sprintf "Boo, a failure: %A" e

//> withFail "hello world";;
//> val it : string =
//  "Boo, a failure: System.Exception: I failed! :( - hello world"
// The second two remote calls are never made.