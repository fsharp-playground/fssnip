(* Just a simple A wordlister for now, srv needs to be re-written *)
open System.Diagnostics

open System
open System.Net
open System.Text
open System.Net.Sockets
open System.Threading

let args = Environment.GetCommandLineArgs()

ThreadPool.SetMaxThreads(128,128)
ThreadPool.SetMinThreads(24,22)
printfn "Max Threads: %d" 128
printfn "MinThreads: %d" 22
 
(* Modified from http://alexhumphrey.me.uk/dot-net/host-name-lookups-with-f/ *)
let getHostNamesAsync : string seq -> 'a seq =
    Seq.map (fun hn -> (hn, Dns.BeginGetHostEntry(hn, null, null)))
    >> Seq.toArray
    >> Seq.map (fun (hn,ias) -> try Some(hn, Dns.EndGetHostEntry(ias)) with ex -> None)
    >> Seq.choose id

let printResult (targ,(hn:IPHostEntry)) =
    printfn "%s - %s" targ hn.HostName
 
let get targ = 
    let inputFile = (System.IO.File.ReadAllLines @"dnsbig.txt") 
    printfn "Words to try: %d" (Array.length inputFile)
    inputFile
    |> Array.map(fun x -> sprintf "%s.%s" x targ)
    |> getHostNamesAsync
    |> Seq.iter printResult

if args.Length < 2 then printf "dns.exe host"
else 
    let sWatch = new Stopwatch()
    sWatch.Start()
    get  args.[1]
    let ts = sWatch.Elapsed
    let elapsedTime = 
        String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10)
    Console.WriteLine("RunTime " + elapsedTime)