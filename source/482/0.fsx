// [snippet: Coroutine]
module Coroutine

// requires the monad library
open FSharp.Monad.Continuation
open System.Collections.Generic

type Coroutine<'T>() =
    let tasks = new Queue<Cont<'T,'T>>()
                
    member this.Put(task) =
        let withYield = 
            callCC <| fun exit ->
                task <| fun value -> 
                    callCC <| fun c -> 
                        tasks.Enqueue(c())
                        exit(value)
        tasks.Enqueue(withYield)

    member this.Run() =
        runCont (tasks.Dequeue()) id raise
// [/snippet]


// [snippet: Example]
let coroutine = Coroutine()

coroutine.Put(fun yield' -> cont {
    do! yield'(1)
    do! yield'(2)
    return 3
})

coroutine.Put(fun yield' -> cont {
    do! yield'(10)
    return 20
})

coroutine.Run() |> printfn "%A"
coroutine.Run() |> printfn "%A"
coroutine.Run() |> printfn "%A"
coroutine.Run() |> printfn "%A"
coroutine.Run() |> printfn "%A"
// [/snippet]