open System 

module Chat =        
    type Message =
        | Login of string * string * AsyncReplyChannel<bool>  // name, colour
        | Poll of string * int * AsyncReplyChannel<(int * ((string * string * string) list) * string list) option>    // last seen message time, (name * message * colour) list, 
        | Talk of string * string   // name, message
        
    type LiiState =
        { lastCleanup : DateTime
          users       : Map<string,(DateTime*string)>  // user map keyed by name, value is tuple cotaining the last polled time for the user and their text colour
          messages    : (int * string * string) list }
               
    let mailbox = MailboxProcessor<Message>.Start( fun inbox -> // mailbox enforces concurrent sync
        let rnd = System.Random(DateTime.Now.Millisecond)
        let rec loop initalState = async {
            let! msg = inbox.TryReceive(timeout=100)
            let state = 
                 if DateTime.Now - initalState.lastCleanup > TimeSpan.FromMinutes(1.) then 
                    let users = initalState.users |> Map.filter( fun user (t,c) -> user = "LII" || DateTime.Now - t <= TimeSpan.FromMinutes(1.) )
                    {initalState with lastCleanup = DateTime.Now; users = users }
                 else initalState
            let state = 
                match msg with
                | Some(Login(user,colour,reply)) -> 
                    match state.users.ContainsKey user with
                    | true -> reply.Reply(false)
                              state
                    | false -> reply.Reply(true)
                               { state with users = state.users.Add(user,(DateTime.Now,colour)) }                        
                | Some(Poll(user,time,reply)) -> 
                    // return the messages this user has yet to see and update the state with the user's new poll time
                    match state.users.ContainsKey user with
                    | true ->
                        let (d,c) = state.users.[user]
                        let messages = ([],state.messages) ||> List.fold( fun acc (t,m,c) -> if t > time then (t,m,c)::acc else acc ) |> List.rev // todo: can write manual recusrion here and exit as soon as first message is encoutnered that the user has already seen
                        let latest = if messages.Length = 0 then time else messages |> List.maxBy( fun (t,_,_) -> t ) |> fun (t,_,_) -> t                            
                        reply.Reply(Some (latest, messages 
                                                    |> List.map(fun (_,n,m) ->                                                         
                                                        let c = match state.users.ContainsKey (n) with
                                                                | true ->  snd state.users.[n]
                                                                | _ -> "#000000"
                                                        (n,m,c)), ([],state.users) ||> Map.fold( fun acc k v -> k::acc) ))
                        { state with users = state.users |> Map.remove user |> Map.add user (DateTime.Now,c) }
                    | false -> reply.Reply(None)
                               state
                | Some(Talk(user,message)) -> 
                    let state = 
                        match state.users.ContainsKey user with
                        | true -> 
                            let t = match state.messages with
                                    | (x,_,_)::_ -> x
                                    | _ -> 0                            
                            let messages = (t+1,user,message)::state.messages
                            match messages.Length with
                            | x when x > 150 -> { state with messages = messages |> Seq.take 150 |> Seq.toList }
                            | _ -> { state with messages = messages }
                        | false -> state 
                    if user <> "LII" then                           
                        let msg = message.Trim().ToLower()
                        let response = LII.handleInput msg                        
                        let respond() = async {  
                            do! Async.Sleep(rnd.Next(500,2500))
                            inbox.Post(Talk("LII",response)) } |> Async.Start
                        match rnd.NextDouble() * 100. with
                        | x when x <= 15. -> respond()
                        | x when x <= 50. && msg.Contains("lii") -> respond()
                        | x when x <= 90. && msg.Contains("lii") && msg.EndsWith("?") -> respond()
                        | _ -> ()
                    state
                | None -> state
                    
                    
            do! Async.Sleep(100)
            return! loop state
        }

        let initialState =
            { lastCleanup = DateTime.Now
              users = [("LII",(DateTime.Now,"#000000"))] |> Map.ofList
              messages = [] }

        loop initialState )
