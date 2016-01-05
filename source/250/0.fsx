let ``BlockingCollection should block if empty``() =
    par {
        let col = new BlockingCollection<int>()
        let ctx = new ParContext()
        do! ctx.ChangeTo 0
        col.Add 1
        do! ctx.ChangeTo 1
        assert(col.Take() = 1)
        do! ctx.ChangeToAfterBlocked 0 (fun() -> assert(col.Take() = 2))
        col.Add 2
    }





// Teh source... nicht wirklich interessant oder lesbar

open System
open System.Threading
open System.Collections.Generic
open System.Collections.Concurrent

type ParThread(name) =
    let conts = new BlockingCollection<unit -> unit>()
    let thread = Thread(fun() ->
        let rec loop() =
            match conts.TryTake(millisecondsTimeout = -1) with
            | (true,cont) -> cont(); loop()
            | _ -> ()
        loop()
    )
    do
        thread.Name <- name
        thread.Start()
    member x.Run cont = conts.Add cont
    member x.IsBlocked = thread.ThreadState = ThreadState.WaitSleepJoin

type ParContext() =
    let threads = Dictionary<int, ParThread>()
    member x.ChangeTo n =
        match threads.TryGetValue n with
        | (true,t) -> t
        | _ ->
            let t = ParThread(n.ToString())
            threads.Add(n,t)
            t
    member x.ChangeToAfterBlocked n blocker = x.ChangeTo n, blocker
        
type ParBuilder() =
    let mutable current : ParThread option = None
    member x.Bind(thread : ParThread, cont) =
        current <- Some thread
        thread.Run cont
    member x.Bind((thread : ParThread, blocker: unit -> unit), cont) =
        ThreadPool.QueueUserWorkItem(fun _ ->
            let current = Option.get current
            current.Run blocker
            while not current.IsBlocked do
                Thread.Sleep 100
            x.Bind(thread, cont)
        ) |> ignore
    member x.Zero() = ()

let par = ParBuilder()

let ``BlockingCollection should block if empty``() =
    par {
        let col = new BlockingCollection<int>()
        let ctx = new ParContext()
        do! ctx.ChangeTo 0
        col.Add 1
        do! ctx.ChangeTo 1
        assert(col.Take() = 1)
        do! ctx.ChangeToAfterBlocked 0 (fun() -> assert(col.Take() = 2))
        col.Add 2
    }