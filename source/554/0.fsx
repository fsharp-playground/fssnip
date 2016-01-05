open System

   

 type ActorException(exn, actor : IActor) =
      inherit Exception(sprintf "An exception occured in Actor(%s), please see inner exception for more details" actor.Id, exn) 

      member x.Actor
        with get() = actor

    and ActorMessage<'msg> =
      | Message of 'msg
      | ParameterizedQuery of AsyncReplyChannel<obj> * 'msg
      | FrameworkMessage of ActorFwkMessage
    and ActorFwkMessage = 
      | Stop
      | Query of AsyncReplyChannel<obj>
      | Restart

    and MessageLoop<'msg, 'state> = (MailboxProcessor<ActorMessage<'msg>> -> 'state -> Async<unit>) 

    and IActor =
        abstract Id : string with get
        [<CLIEvent>]
        abstract Error : IEvent<ActorException> with get
        abstract Post : 'a -> unit
        abstract PostAndReply<'b> : 'a -> 'b
        abstract PostControlMessage : ActorFwkMessage -> unit 
        abstract Query<'a> : unit -> 'a

    and Actor<'msg, 'state>(id, messageLoop : MessageLoop<'msg, 'state>, state) as self = 
                 
      let mutable initialState = state
      let mutable _messageLoop = messageLoop
      let errorStream = new Event<ActorException>()
      let mutable id = 
          let tId = defaultArg id (Guid.NewGuid().ToString())
          if String.IsNullOrEmpty(tId)
          then Guid.NewGuid.ToString()
          else tId
      
      let mutable actor = MailboxProcessor<ActorMessage<'msg>>.Start(fun inbox -> messageLoop inbox initialState)    

      let reBuildProcessor() =
          actor <- MailboxProcessor<ActorMessage<'msg>>.Start(fun inbox -> messageLoop inbox initialState)   

      do 
        actor.Error.Add(fun exn -> reBuildProcessor()
                                   errorStream.Trigger(ActorException(exn, self)))
      
      
      override x.ToString() =
            (x :> IActor).Id

      member x.MessageLoop 
            with get() = _messageLoop 
            and set(v) = 
                _messageLoop <- v
                reBuildProcessor()

      member x.InitialState 
            with get() = initialState 
            and set(v) = 
                initialState <- v
                reBuildProcessor()


      interface IActor with
        member x.Id with get() = id

        [<CLIEvent>]
        member x.Error
            with get() = errorStream.Publish

        member x.Post(msg) =
            actor.Post(Message(unbox<'msg>(box(msg))))

        member x.PostControlMessage(msg : ActorFwkMessage) = 
            actor.Post(FrameworkMessage(msg))

        member x.PostAndReply<'b>(msg) = 
            actor.PostAndReply(fun rc -> ParameterizedQuery(rc,unbox<'msg>(box(msg)))) :?> 'b

        member x.Query<'a>() =
             actor.PostAndReply(fun rc -> FrameworkMessage(Query(rc))) :?> 'a
 type RegistryMessage =
    | Register of IActor
    | UnRegister of IActor
    | Dispose

  type RegistryEvent =
    | OnPostControlMessage of IActor * ActorFwkMessage
    | OnRegister of IActor
    | OnUnregister of IActor
    | OnPost of IActor * obj 
    | OnPostAndReply of IActor * obj
    | OnQuery of IActor
    | OnError of Exception

  type Registry() =

    static let registryEvent = new Event<RegistryEvent>() 

    static let fireRegistryCallback(event : RegistryEvent) =
        async
            {   
                try
                    registryEvent.Trigger(event)
                with
                    e -> ()
            } |> Async.Start

    static let messageHandler (msg:RegistryMessage) (state:Map<string,IActor>) =
        match msg with
        | Register(ref) ->
             fireRegistryCallback(OnRegister(ref)) 
             state.Add(ref.Id, ref)
        | UnRegister(ref) ->
             fireRegistryCallback(OnUnregister(ref)) 
             state.Remove ref.Id
        | Dispose ->
            state |> Map.iter (fun _ c -> c.PostControlMessage(Stop))
            Map.empty
     
    static let defaultErrorHandler id exn = 
            registryEvent.Trigger(OnError(ActorException(exn, id)))

    static let registrationActor = Actor<RegistryMessage, Map<string,IActor>>(Some <| "Registry",  MessageLoopImpl.defaultMessageLoop messageHandler, Map.empty<string,IActor>) :> IActor
    
    static let actorFunc f target =
        let actors : Map<string,IActor> = registrationActor.Query()
        match actors.TryFind target with
        | Some(ref) -> Some <| f(ref)
        | None -> None 

    [<CLIEvent>]
    static member RegistryEvent
        with get() = registryEvent.Publish 
        
    static member Register(actorRef : IActor) =
        registrationActor.Post(Register(actorRef))

    static member UnRegister(actorRef : IActor) =
        registrationActor.Post(UnRegister(actorRef))

    static member PostControlMessage (target : string) (msg) = 
         actorFunc (fun ref -> 
                            ref.PostControlMessage(msg)
                            fireRegistryCallback(OnPostControlMessage(ref, msg))
                   ) target
         |> Option.isSome
    
    static member Post (target : string) (msg) =
         match target with 
         | "*" ->
            Registry.PostToAll msg
            true
         | _ ->
            actorFunc (fun ref -> 
                               ref.Post(msg)
                               fireRegistryCallback(OnPost(ref, msg))
                      ) target
            |> Option.isSome 
     
    static member PostToAll msg =
        let actors : Map<string,IActor> = registrationActor.Query()
        actors |> Map.iter (fun k v -> v.Post(msg))
        
    static member PostAndReply (target : string) (msg) = 
         actorFunc (fun ref -> 
                        let res = ref.PostAndReply()
                        fireRegistryCallback(OnPostAndReply(ref, msg))
                        res
          ) target

    static member Query (target : string) = 
         actorFunc (fun ref -> 
                        let res = ref.Query()
                        fireRegistryCallback(OnQuery(ref))
                        res
          ) target

    static member Dispose() =
        registrationActor.Post(Dispose) |> ignore
        registrationActor.PostControlMessage(Stop) |> ignore
