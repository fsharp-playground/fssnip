open System
open System.Collections.Generic

type internal AggregateAction = 
    {
        Id: Guid
        Action: Action
    }

type AggregateAgent() =
    let agent = MailboxProcessor<AggregateAction>.Start(fun inbox ->
        let dic = new Dictionary<Guid,MailboxProcessor<Action>>()
        async{                                                                                                  
                while true do
                    let! msg = inbox.Receive()
                    if dic.ContainsKey(msg.Id) then 
                        dic.Item(msg.Id).Post msg.Action
                    else
                        let aggAgent = MailboxProcessor<Action>.Start( fun inbox ->
                            async{
                                while true do
                                    let! action = inbox.Receive()
                                    action.Invoke() |> ignore     
                            })
                        aggAgent.Post msg.Action
                        dic.Add(msg.Id,aggAgent)
        })
        
    member this.Process id action = 
        agent.Post { Id = id; Action = action}
