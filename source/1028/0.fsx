// Atom definition

open System.Threading

type Atom<'T when 'T : not struct> (value : 'T) =
    let cell = ref value
    
    let rec swap f = 
        let currentValue = !cell
        let result = Interlocked.CompareExchange<'T>(cell, f currentValue, currentValue)
        if obj.ReferenceEquals(result, currentValue) then ()
        else Thread.SpinWait 20; swap f

    let transact f =
        let output = ref Unchecked.defaultof<'S>
        let f' x = let t,s = f x in output := s ; t
        swap f' ; !output
        
    member __.Value = !cell
    member __.Swap (f : 'T -> 'T) : unit = swap f
    member __.Transact (f : 'T -> 'T * 'S) : 'S = transact f

// implementation of the caching mechanism

open System

type Cache<'T> (factory : unit -> 'T, ?timeToLive : int) =
    let ttl = defaultArg timeToLive 1000 |> float |> TimeSpan.FromMilliseconds
    let container = Atom<(Choice<'T,exn> * DateTime) option> None

    member __.Value =
        let update () =
            let value = try factory () |> Choice1Of2 with e -> Choice2Of2 e
            Some (value, DateTime.Now), value
        
        let result =
            container.Transact(
                function
                | None -> update ()
                | Some(_, time) when DateTime.Now - time > ttl -> update ()
                | Some(value, _) as state -> state, value)

        match result with
        | Choice1Of2 v -> v
        | Choice2Of2 e -> raise e

// example

let cache = new Cache<_>(fun () -> printfn "computing..."; 42)

for _ in 1 .. 1000000 do
    cache.Value |> ignore