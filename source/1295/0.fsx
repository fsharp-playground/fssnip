module ManyProc

open System.Diagnostics

type OK = OK
type Agent<'T> = MailboxProcessor<'T>

let spawn f howManyMore pid = 
    Agent<OK>.Start(fun inbox -> async {
        let! _ = Async.StartChild <| f howManyMore pid
        let! _ = inbox.Receive()
        ()
    })

let rec startProc howManyMore (pid: Agent<OK>) =
    async {
        match howManyMore, pid with
        | 0, pid -> pid.Post OK
        | howManyMore, pid ->
            let newPid = spawn startProc (howManyMore - 1) pid
            newPid.Post OK
    }

let start howMany f =
    let pid =
        Agent<OK>.Start(fun inbox -> async {
            let! _ = inbox.Receive()
            f()
        })
    startProc howMany pid

let startAndTime howMany =
    let stopwatch = Stopwatch.StartNew()
    let callback() =
        stopwatch.Stop()
        printfn "Completed in %i ms" stopwatch.ElapsedMilliseconds
    start howMany callback |> Async.StartImmediate
