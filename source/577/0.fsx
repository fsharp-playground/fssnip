type 'a M =
    | Ready of 'a
    | Sleeping of unsigned * (unit -> 'a M)
    | Tick of (unit -> 'a M)
    | Interruptible of (unit -> 'a M)
    | Done of 'a M

type TaskBuilder() =
    let rec bind (k : 'a -> 'b M) (v : 'a M) : 'b M =
        match v with
        | Ready d -> k d
        | Sleeping(t, f) -> Sleeping(t, fun () -> f () |> bind k)
        | Tick f -> Tick(fun () -> f () |> bind k)
        | Interruptible f -> Interruptible(fun () -> f () |> bind k)
        | Done r -> Done(r |> bind k)