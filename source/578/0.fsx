[<RequireQualifiedAccess>]
module Task
open System
open System.Collections.Generic

type private unsigned = uint64

type 'a M =
    | Delay of (unit -> 'a M)
    | Sleeping of unsigned * (unit -> 'a M)
    | Tick of (unit -> 'a M)
    | Interruptible of (unit -> 'a M)
    | Done of 'a

type 'a Entity(e, m, id) =
    member val Entity : 'a = e with get
    member val State : _ M = m with get, set
    member val Id : unsigned = id with get

type private TaskComparer<'a>() =
    interface IComparer<'a Entity> with
        member o.Compare(x, y) =
            let c1, o1, c2, o2 = x.Id, x.State, y.Id, y.State
            let cmp = Comparer.Default.Compare
            match o1, o2 with
            (* always last *)
            | Interruptible _, Interruptible _ -> cmp(c1, c2)
            | Interruptible _, _ -> 1
            | _, Interruptible _ -> -1
            (* never contained in the task list *)
            | Done _, _ -> failwith "can't happen"
            | _, Done _ -> failwith "can't happen"
            | Delay _, Delay _ -> cmp(c1, c2)
            | Delay _, _ -> -1
            | _, Delay _ -> 1
            | Tick _, Tick _ -> cmp(c1, c2)
            | Tick _, _ -> -1
            | _, Tick _ -> 1
            | Sleeping (t1, _), Sleeping (t2, _) -> let ret = cmp(t1, t2)
                                                    if ret = 0 then cmp(c1, c2)
                                                    else ret

type TaskBuilder<'TEntity>() =
    (* builder helpers *)
    let rec bind (k : 'a -> 'b M) (v : 'a M) : 'b M =
        match v with
        | Delay d -> d () |> bind k
        | Sleeping(t, f) -> Sleeping(t, fun () -> f () |> bind k)
        | Tick f -> Tick(fun () -> f () |> bind k)
        | Interruptible f -> Interruptible(fun () -> f () |> bind k)
        | Done r -> k r
    let delay (k : unit -> 'a M) : 'a M =
        Delay k
    let ret (r : 'a) : 'a M =
        Done r
    let rec whileLoop (cond : unit -> bool) (expr : 'a M) : unit M =
        if cond ()
        then expr |> bind (fun _ -> whileLoop cond expr)
        else Done ()
    let forLoop (lst : 'a seq) (body : 'a -> 'b M) =
        let iter = lst.GetEnumerator ()
        Delay(fun () -> iter.Current |> body) |> whileLoop (fun () -> iter.MoveNext ())
    (* scheduling helpers *)
    let mutable offset = 0UL
    let mutable last = Int32.MinValue
    let tasks = SortedSet(TaskComparer())
    let ticks () =
        let ticks = Environment.TickCount
        if ticks < last
        then offset <- offset + (uint64 UInt32.MaxValue)
        last <- ticks
        offset + (uint64 ticks)
    let peek ticks (task : _ M) =
        match task with
        | Done _ -> failwith "can't happen"
        | Delay _ -> true
        | Sleeping(t, _) -> t < ticks
        | Tick _ -> true
        | Interruptible _ -> false
    let eval (task : 'a M) : 'a M =
        match task with
        | Delay f -> f ()
        | Done v -> task
        | Interruptible f -> f ()
        | Sleeping(t, f) -> f ()
        | Tick f -> f ()
    let mutable counter = 0UL
    let makeId () = let ret = counter
                    counter <- counter + 1UL
                    ret
    (* builder implementation *)
    member o.Bind(v, k) = bind k v
    member o.Return(r) = ret r
    member o.Delay k = delay k
    member o.While(cond, expr) = whileLoop cond expr
    member o.Zero () = Done ()
    member o.For(lst, body) = forLoop lst body
    member o.Combine(first, second) = bind (fun () -> second) first
    (* scheduling implementation *)
    member o.add (entity : 'TEntity) (expr : _ M) =
        let entity = Entity(entity, expr, makeId ())
        tasks.Add(entity) |> ignore
        entity
    member o.interrupt (entity : 'TEntity Entity) =
        match entity.State with
            | Interruptible m -> tasks.Remove(entity) |> ignore
                                 entity.State <- m ()
                                 tasks.Add(entity) |> ignore
                                 true
            | _ -> false
    member o.sleep dt =
        if dt < 0 then failwith "delta time can't be less than 0"
        let t = (uint64 dt) + ticks ()
        Sleeping(t, fun () -> Done ())
    member o.tick () =
        let ticks = ticks ()
        if tasks.Count > 0 then
            let nextTick = ResizeArray()
            while tasks.Count > 0 && peek ticks tasks.Min.State do
                let entity = tasks.Min
                let task = entity.State
                tasks.Remove(entity) |> ignore
                let m = eval task
                match m with
                | Tick _ -> entity.State <- m
                            nextTick.Add(entity) |> ignore
                | Done _ -> ()
                | _ -> entity.State <- m
                       tasks.Add(entity) |> ignore
            nextTick |> Seq.iter (fun m -> tasks.Add(m) |> ignore)
    //member o.test () = Tick(fun () -> Done 42)

let task : unit TaskBuilder = TaskBuilder()

Console.ReadLine () |> ignore