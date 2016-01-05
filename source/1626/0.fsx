open System
open EventStore.ClientAPI
open Chiron

let store = 
    (ConnectionSettings.Create()
        .UseConsoleLogger()
        .Build(),
     System.Net.IPEndPoint(System.Net.IPAddress.Parse "192.168.33.10", 1113))
    |> EventStoreConnection.Create

store.ConnectAsync().Wait()

let inline encode event =
    Json.serialize event
    |> Json.format 
    |> System.Text.Encoding.UTF8.GetBytes

let inline decode (bytes : byte []) =
    bytes
    |> System.Text.Encoding.UTF8.GetString
    |> fun s ->
        printfn "%s" s
        s
    |> Json.parse
    |> Json.deserialize

let rec inline before currentState eventNumber fold streamName =
    async {
        let! slice =
            store.ReadStreamEventsForwardAsync(streamName, eventNumber, 10, true)
            |> Async.AwaitTask
        let nextState =
            slice.Events
            |> Seq.map (fun e -> e.Event.Data |> decode)
            |> Seq.fold fold currentState
        if slice.IsEndOfStream then
            return (nextState, slice.NextEventNumber)
        else
            return! refresh' nextState slice.NextEventNumber fold streamName
    }

let inline after currentState eventNumber fold streamName =
    let rec inner currentState eventNumber fold streamName =
        async {
            let! slice =
                store.ReadStreamEventsForwardAsync(streamName, eventNumber, 10, true)
                |> Async.AwaitTask
            let nextState =
                slice.Events
                |> Seq.map (fun e -> e.Event.Data |> decode)
                |> Seq.fold fold currentState
            if slice.IsEndOfStream then
                return (nextState, slice.NextEventNumber)
            else
                return! inner nextState slice.NextEventNumber fold streamName
        }
    inner currentState eventNumber fold streamName
