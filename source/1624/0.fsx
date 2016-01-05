open System.IO
open System.Collections.Concurrent

let q = ConcurrentQueue<int>()

let initialGameState = 0

let renderWorld state =
    printfn "world state %d" state

let rec physicsEngine state =
    match q.Count with
    | 0 -> state
    | _ ->
        let mutable update = 0
        if q.TryDequeue(&update) then physicsEngine(state + update) else physicsEngine(state)  

let rec evolveWorld(state)  = async {

    do! Async.Sleep(1000)

    let newState = physicsEngine(state)

    renderWorld(newState)

    Async.Start <| evolveWorld(newState)
}

evolveWorld(initialGameState) |> Async.Start 


let rand = System.Random()
while true do
    System.Threading.Thread.Sleep(rand.Next(0,1000))
    q.Enqueue(1)
