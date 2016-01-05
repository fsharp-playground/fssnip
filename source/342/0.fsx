(* Windows Evenlog Iterator *)
(* TODO: Op codes for eventID *)

open System
open System.Diagnostics

#r "FSharp.PowerPack.Parallel.Seq";;
open Microsoft.FSharp.Collections
open System.Linq

let logEnt (event, desc) = 
    async { 
        (new EventLog(event, ".")).Entries
            |> Seq.cast
            |> PSeq.filter(fun (x:EventLogEntry) -> x.InstanceId = desc )
            |> PSeq.iter(fun x -> printfn "%A" x.TimeGenerated.TimeOfDay) 
    }
    
let getLogData eventList =
        eventList
            |> Seq.map logEnt       
            |> Async.Parallel
            |> Async.RunSynchronously   
            |> ignore          

(* Software Protection Service *)            
getLogData[("application", 1033L)]
(* Real: 00:00:10.870, CPU: 00:00:05.928, GC gen0: 7, gen1: 1, gen2: 0*)

(* Depending on the type of event you are looking to get, 
   it is important to chose the correctly log for the given event.
   For example the below uses the security log to get logon info.
   Current secrurity log has 30,000 entries, this makes the iteration halve in speed.
   (in comparison to application log which has over 60,000 entries)
*)  

(* Logon *)
getLogData[("security", 4624L)]
(* Real: 00:00:06.945, CPU: 00:00:10.639, GC gen0: 20, gen1: 8, gen2: 0 *)        