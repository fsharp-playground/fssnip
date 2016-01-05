open System
open FSharp.Control

let event = new Event<_>()

let createObservableByExecutingAsync asyncToExecute =
    event.Publish
    |> Observable.guard(fun _ -> 

        let asyncOperation = async {
            asyncToExecute
            |> Async.RunSynchronously

            event.Trigger(())
        }
        
        printfn "Start Executing Async Operation"

        Async.Start asyncOperation

        printfn "End Executing Async Operation"
    )

let someAsyncOperation = async {
        printfn "Async Operation"
    }

let asyncWorker = createObservableByExecutingAsync someAsyncOperation

printfn "Start Waiting for Async Result"

Async.AwaitObservable asyncWorker
|> Async.RunSynchronously

printfn "End Waiting for Async Result"

Console.ReadLine()
|> ignore