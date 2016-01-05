// Create Console-application, then NuGet: Install-Package Akka
module AkkaConsoleApplication
open Akka
open Akka.Actor
 
type Greet(who) =
    member x.Who = who
 
type GreetingActor() as g =
    inherit ReceiveActor()
    do g.Receive<Greet>(fun (greet:Greet) -> printfn "Hello %s" greet.Who)
 
[<EntryPoint>]  // Works also from F#-Interactive.
let main argv = // More details: http://getakka.net/wiki/Getting%20started
    let system = ActorSystem.Create "MySystem"
    let greeter = system.ActorOf<GreetingActor> "greeter"
    "World" |> Greet |> greeter.Tell
    System.Console.ReadLine() |> ignore
    0 // return an integer exit code