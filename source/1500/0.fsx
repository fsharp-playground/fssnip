module Projection

type Message = 
    | Update
    | Rebuild

/// <summary>Build an actor to coordinate taking events and transforming (projecting) them into a new representation</summary>
/// <param name="getNextEvent">Get the next event after the id if specified, if not specified get the first event</param>
/// <param name="getCurrentState">Get the current state of the projection, or None if not yet started</param>
/// <param name="updateState">Store the projection if the event id (if specified) matches the currently stored id</param>
/// <param name="applyEvent">Apply the event to previous projection to create the new projection</param>
let buildProjector
    (getNextEvent : 'eventId option -> option<'eventId * 'event> Async)
    (getCurrentState : unit -> option<'eventId * 'projection> Async)
    (updateState : 'eventId option -> ('eventId * 'projection) -> unit Async)
    (applyEvent : 'event -> 'projection option -> 'projection Async) = 

    let rec update currentState = 
        async { 
            let currentEventId = (currentState |> Option.map fst)
            let! nextEventOption = getNextEvent currentEventId
            match nextEventOption with
            | None -> return currentState
            | Some(eventId, event) -> 
                let! nextProjection = currentState
                                      |> Option.map snd
                                      |> applyEvent event
                return! update (Some(eventId, nextProjection))
        }
    (new MailboxProcessor<Message>(fun inbox -> 
    let rec processNextMessage() : Async<unit> = 
        async { 
            let! message = inbox.Receive()
            match message with
            | Update -> let! state = getCurrentState()
                        return! state |> applyUpdate
            | Rebuild -> 
                let state = None
                return! state |> applyUpdate
        }
    
    and applyUpdate state = 
        async { 
            let! updated = state |> update
            match updated with
            | None -> return! processNextMessage()
            | Some u -> 
                do! updateState (state |> Option.map fst) u
                return! processNextMessage()
        }
    
    processNextMessage())).Post
