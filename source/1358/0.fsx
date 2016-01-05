#r @"Akka.dll"
#r @"Akka.FSharp.dll"

open Akka.FSharp
open Akka.Actor


type FunActor<'State,'Msg>(f ,startState:'State) =
    inherit Actor()
    let mutable state = startState

    override x.OnReceive(msg) =
        state <- f state ( msg :?> 'Msg)


module Actor =
    let system name =
        ActorSystem.Create(name)

    let spawn (system:ActorSystem) (s:'s) (f: 's -> 'm -> 's)  =
       system.ActorOf(Props(Deploy.Local, typeof<FunActor<'s,'m>>, [f;s])) 

let system = Actor.system "This is Akka !"

type Message =
    | Inc of int
    | Dec of int

let actor = 
    Actor.spawn system 0
    <| fun s ->
        printfn "%d" s
        function
        | Inc n-> s + n
        | Dec n -> s - n

[0..1000] |> List.iter(fun _ -> actor <! Inc 2)
[0..1000] |> List.iter (fun _ -> actor <! Dec 1)
