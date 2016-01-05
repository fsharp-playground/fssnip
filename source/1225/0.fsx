[<AbstractClass>]
type Index = class end
and Bottom = class inherit Index end
and Indexed<'Tag, 'I1, 'I2, 'T when 'I1 :> Index and 'I2 :> Index> = Func of (unit -> 'T)

let inline run (Func f) = f ()

// Type-level numerals
type Peano = abstract Int : int

let num<'T when 'T :> Peano> = System.Activator.CreateInstance<'T> ()

type Zero () = interface Peano with member __.Int = 0
and Succ<'P when 'P :> Peano> () =
    static let n = num<'P>.Int + 1
    interface Peano with member __.Int = n

type One = Succ<Zero>
type Two = Succ<One>
type Three = Succ<Two>
type Four = Succ<Three>

// fixed iterations type constructor
type Iterate<'Body, 'Iterations when 'Body :> Index and 'Iterations :> Peano> = class inherit Index end

// straightforward implementation of the Indexed monad
type IndexedBuilder<'Tag> () =

    member b.Delay (f : unit -> Indexed<'Tag, 'I1, 'I2, 'T>) = Indexed<'Tag, 'I1, 'I2, 'T>.Func(fun () -> run (f ()))
    member b.Return (x : 'T) = Indexed<'Tag, 'I, 'I, 'T>.Func(fun () -> x)
    member b.ReturnFrom (f : Indexed<'Tag,_,_,_>) = f

    member b.Bind (f : Indexed<'Tag, 'I1, 'I2, 'T>, g : 'T -> Indexed<'Tag, 'I2, 'I3, 'U>) =
        Indexed<'Tag, 'I1, 'I3, 'U>.Func(fun () -> let x = run f in run (g x))

    member b.Combine (f : Indexed<'Tag,_,_,unit>, g : Indexed<_,_,_,_>) = b.Bind(f, fun () -> g)

    member b.For(iters : 'T, bodyF : int -> Indexed<'Tag, 'I1, 'I2, unit>) : Indexed<'Tag, Iterate<'I1, 'T>, 'I2, unit> =
        Func(fun () -> for i = 0 to iters.Int - 1 do run (bodyF i))

// the dual Indexed monad
type DualIndexedBuilder<'Tag> () =

    member b.Delay (f : unit -> Indexed<'Tag, 'I1, 'I2, 'T>) = Indexed<'Tag, 'I1, 'I2, 'T>.Func(fun () -> run (f ()))
    member b.Return (x : 'T) = Indexed<'Tag, 'I, 'I, 'T>.Func(fun () -> x)
    member b.ReturnFrom (f : Indexed<'Tag,_,_,_>) = f

    // composition of indices done with reversed arrows
    member b.Bind (f : Indexed<'Tag, 'I2, 'I1, 'T>, g : 'T -> Indexed<'Tag, 'I3, 'I2, 'U>) =
        Indexed<'Tag, 'I3, 'I1, 'U>.Func(fun () -> let x = run f in run (g x))

    member b.Combine (f : Indexed<'Tag,_,_,unit>, g : Indexed<'Tag,_,_,_>) = b.Bind(f, fun () -> g)

    member b.For(iters : 'Iterations, bodyF : int -> Indexed<'Tag, 'I1, 'I2, unit>) : Indexed<'Tag, Iterate<'I1, 'Iterations>, 'I2, unit> =
        Func(fun () -> for i = iters.Int - 1 downto 0 do run (bodyF i))

// two indexed computations match iff their precomposition and postcomposition indices coincide
type Indexed =
    static member Match (f : Indexed<_, 'I1, 'I2, _>) (g : Indexed<_, 'I1, 'I2, _>) = ()


// Branching

type Branch<'B1, 'B2 when 'B1 :> Index and 'B2 :> Index> = class inherit Index end
type Branch<'B1, 'B2, 'B3 when 'B1 :> Index and 'B2 :> Index and 'B3 :> Index> = class inherit Index end

type Branch =
    static member B1Of2 () = Indexed<'Tag, Branch<'B1, 'B2>, 'B1, unit>.Func id
    static member B2Of2 () = Indexed<'Tag, Branch<'B1, 'B2>, 'B2, unit>.Func id

    static member B1Of3 () = Indexed<'Tag, Branch<'B1, 'B2, 'B3>, 'B1, unit>.Func id
    static member B2Of3 () = Indexed<'Tag, Branch<'B1, 'B2, 'B3>, 'B3, unit>.Func id
    static member B3Of3 () = Indexed<'Tag, Branch<'B1, 'B2, 'B3>, 'B3, unit>.Func id


// Custom Operations

type Op<'Name, 'Rest when 'Rest :> Index> = class inherit Index end
type Op<'Name, 'Param, 'Rest when 'Rest :> Index> = class inherit Index end

type Operation<'Tag, 'Name> =
    static member Do (f : unit -> 'T) = Indexed<'Tag, Op<'Name, 'Rest>, 'Rest, 'T>.Func f

type Operation<'Tag, 'Name, 'Param> =
    static member Do (f : unit -> 'T) = Indexed<'Tag, Op<'Name, 'Param, 'Rest>, 'Rest, 'T>.Func f

//
//  Examples
//

// 1. session types: a rudimentary client-server scheme

type Send = class end
type Receive = class end

type Choose = class end
type Offer = class end

type Channel () =
    let q = new System.Collections.Queue()

    member __.Send (x : 'a) = q.Enqueue x
    member __.Receive<'a> () =
        while q.Count = 0 do System.Threading.Thread.SpinWait 20
        q.Dequeue () :?> 'a

type Client (cc : Channel, sc : Channel) =
    member __.Send (x : 'T) = Operation<Client, Send, 'T>.Do(fun () -> sc.Send x)
    member __.Receive () = Operation<Client, Receive, 'T>.Do(fun () -> cc.Receive<'T>())

    member __.Offer () = Operation<Client, Offer>.Do(fun () -> cc.Receive<bool>())
    member __.Choose (value : bool) = Operation<Client, Choose>.Do(fun () -> sc.Send value)

and Server (cc : Channel, sc : Channel) =
    member __.Send (x : 'T) = Operation<Server, Receive, 'T>.Do(fun () -> cc.Send x)
    member __.Receive () = Operation<Server, Send, 'T>.Do(fun () -> sc.Receive<'T>())

    member __.Choose (value : bool) = Operation<Server, Offer>.Do(fun () -> cc.Send value)
    member __.Offer () = Operation<Server, Choose>.Do(fun () -> sc.Receive<bool>())

and Session () =
    let cc = new Channel()
    let sc = new Channel()

    let client = new Client(cc, sc)
    let server = new Server(cc, sc)
    
    member __.Client = client
    member __.Server = server


let server = new IndexedBuilder<Server> ()
let client = new IndexedBuilder<Client> ()


let df () = Unchecked.defaultof<_>

let runClientServer (clientF : Client -> Indexed<Client,_,Bottom,'T>) (serverF : Server -> Indexed<Server,_,_,_>) =
    // for type checker only
    let attest () = Indexed.Match (clientF (df())) (serverF (df()))

    let session = new Session ()

    [| async { return run (clientF session.Client) :> obj } ; 
       async { return run (serverF session.Server) :> obj } |]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> fun x -> x.[0] :?> 'T


// sample

let clientWorkflow (i : int) (j : int) (self : Client) =
    client {
        do! self.Send i
        do! self.Send j
        let! result = self.Offer ()
        if result then
            do! Branch.B1Of2 ()
            let! msg = self.Receive ()
            do printfn "server replied with error: %s" msg
            return None
        else
            do! Branch.B2Of2 ()
            let! x = self.Receive ()
            return Some x
    }

let serverWorkflow (self : Server) =
    server {
        let! i = self.Receive ()
        let! j = self.Receive ()
        do! self.Choose ((j = 0))
        if j = 0 then
            do! Branch.B1Of2 ()
//            do! self.Send 12 // comment out to create a type error
            do! self.Send "cannot divide by zero!"
        else
            do! Branch.B2Of2 ()
            return! self.Send (i / j)
    }


runClientServer (clientWorkflow 500 2) serverWorkflow
runClientServer (clientWorkflow 500 0) serverWorkflow


// 2. serializers: defining type-safe serialization rules.

open System.IO

type Serialize = class end
type Deserialize = class end

type Primitive = class end

let serializer = new IndexedBuilder<Serialize> ()
let deserializer = new IndexedBuilder<Deserialize> ()

type Writer(m : MemoryStream) =
    do m.Position <- 0L
    let bw = new BinaryWriter(m)

    member __.Write (x : 'T) =
        let write () =
            match box x with
            | :? bool as b -> bw.Write b
            | :? int as n -> bw.Write n
            | :? float as f -> bw.Write f
            | :? string as s -> bw.Write s
            | _ -> invalidArg typeof<'T>.Name "I do not know how to serialize this type."

        Operation<Serialize, Primitive, 'T>.Do write

    member __.Choose1Of2 () = 
        serializer {
            do! Operation<Serialize, Choose>.Do(fun () -> bw.Write true)
            do! Branch.B1Of2 ()
        }

    member __.Choose2Of2 () =
        serializer {
            do! Operation<Serialize, Choose>.Do(fun () -> bw.Write false)
            do! Branch.B2Of2 ()
        }


type Reader(m : MemoryStream) =
    do m.Position <- 0L
    let br = new BinaryReader(m)

    member __.Read () =
        let read () =
            match typeof<'T> with
            | t when t = typeof<bool> -> br.ReadBoolean() :> obj :?> 'T
            | t when t = typeof<int> -> br.ReadInt32() :> obj :?> 'T
            | t when t = typeof<float> -> br.ReadDouble() :> obj :?> 'T
            | t when t = typeof<string> -> br.ReadString() :> obj :?> 'T
            | _ -> invalidArg typeof<'T>.Name "I do not know how to deserialize this type."

        Operation<Deserialize, Primitive, 'T>.Do read

    member __.Offer () = Operation<Deserialize, Choose>.Do(fun () -> br.ReadBoolean())


let runSerializationLoop (ser : 'T -> Writer -> Indexed<Serialize,_,Bottom,_>) (dser : Reader -> Indexed<Deserialize,_,_,'T>) (x : 'T) =
    let attest () = Indexed.Match (ser (df ()) (df())) (dser (df()))
    
    use m = new MemoryStream ()
    run <| ser x (Writer(m))
    run <| dser (Reader(m))


let serialize (c : Choice<'T * 'U, 'S option>) (w : Writer) =
    serializer {
        match c with
        | Choice1Of2 (t,u) ->
            do! w.Choose1Of2 ()

            do! w.Write t
            do! w.Write u

        | Choice2Of2 sOpt ->
            do! w.Choose2Of2 ()

            match sOpt with
            | None ->
                do! w.Choose1Of2 ()
            | Some s ->
                do! w.Choose2Of2 ()
                do! w.Write s
    }

let deserialize (r : Reader) =
    deserializer {
        let! branch = r.Offer ()
        if branch then
            do! Branch.B1Of2 ()

            let! t = r.Read ()
            let! u = r.Read ()

            return Choice1Of2(t, u)
        else
            do! Branch.B2Of2 ()
            let! branch = r.Offer ()
            if branch then
                do! Branch.B1Of2 ()
                return Choice2Of2 None
            else
                do! Branch.B2Of2 ()
                let! s = r.Read()
                return Choice2Of2 (Some s)
    }

let x = Choice2Of2 (Some 3) : Choice<int * string,_>

runSerializationLoop serialize deserialize x


// 3. reversible computation : building workflows that are provably reverse.

// encodes an atomic operation that is reversible by definition
type ReversiblePrimitive =
    {
        Do : unit -> unit
        Undo : unit -> unit
    }

let verbose msg = 
    { 
        Do = fun () -> printfn "doing '%s'." msg
        Undo = fun () -> printfn "undoing '%s'." msg 
    }

type Forward = class end
type Reverse = class end

type Forward<'Name> =
    static member Run (f : ReversiblePrimitive) = Operation<Forward, 'Name>.Do f.Do

type Reverse<'Name> =
    static member Undo (f : ReversiblePrimitive) = Operation<Reverse, 'Name>.Do f.Undo

let forward = new IndexedBuilder<Forward> ()
let reverse = new DualIndexedBuilder<Reverse> ()

let isReverseOf (f : Indexed<Reverse,_,Bottom,_>) (g : Indexed<Forward,_,_,_>) = Indexed.Match g f

type Action1 = class end
type Action2 = class end
type Action3 = class end
type Loop = class end

let install () =
    forward {
        do! Forward<Action1>.Run <| verbose "action 1"
        do! Forward<Action2>.Run <| verbose "action 2"
        do! Forward<Action3>.Run <| verbose "action 3"

        for i in num<Four> do
            do! Forward<Loop>.Run <| verbose (sprintf "Loop, iteration #%d" i)
    }

let uninstall () =
    reverse {

        for i in num<Four> do
            do! Reverse<Loop>.Undo <| verbose (sprintf "Loop, iteration #%d" i)

        do!
            reverse {
                do! reverse {
                    do! Reverse<Action3>.Undo <| verbose "action 3"
                    do! Reverse<Action2>.Undo <| verbose "action 2"
                     
                }
                  
                do! Reverse<Action1>.Undo <| verbose "action 1"
            }
    }

let attest () = isReverseOf (uninstall ()) (install ())

run <| install ()
run <| uninstall ()