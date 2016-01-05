// [snippet: coroutine]
// requires the monad library
open FSharp.Monad.Continuation
open System.Collections.Generic

type Coroutine() =
    let tasks = new Queue<Cont<unit,unit>>()
                
    member this.Put(task) =
        let withYield =
            cont { do! callCC <| fun exit ->
                        task <| fun () -> 
                            callCC <| fun c -> 
                                tasks.Enqueue(c())
                                exit()
                   if tasks.Count <> 0 then
                        do! tasks.Dequeue() }
        tasks.Enqueue(withYield)

    member this.Run() =
        runCont (tasks.Dequeue()) ignore raise
// [/snippet]

// [snippet: coroutine example]
let coroutine = Coroutine()

coroutine.Put(fun yield' -> cont {
    printfn "A"
    do! yield'()
    printfn "B"
    do! yield'()
    printfn "C"
})

coroutine.Put(fun yield' -> cont {
    printfn "1"
    do! yield'()
    printfn "2"
})

coroutine.Run()
// [/snippet]