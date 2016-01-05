[<Struct>]
type ArrayAsList<'a> =
    val arr : 'a []
    val idx : int
    new (inarr: 'a []) = { arr = inarr; idx = 0 }
    new (inarr: 'a [], inidx: int) = { arr = inarr; idx = inidx }
    member inline t.IsEmpty = t.idx >= t.arr.Length
    member inline t.Head = t.arr.[t.idx]
    member inline t.Tail = ArrayAsList<_>(t.arr, t.idx + 1)

let inline (|EmptyArrayAsList|UnconsArrayAsList|) (all: ArrayAsList<_>) =
     if all.IsEmpty then EmptyArrayAsList else UnconsArrayAsList(all.Head, all.Tail)

let inline (|EmptyArrayAsList'|_|) (all: ArrayAsList<_>) = 
    if all.IsEmpty then Some() else None
     
let inline (|UnconsArrayAsList'|_|) (all: ArrayAsList<_>) = 
    if all.IsEmpty then None else Some(all.Head, all.Tail)

let x_arr = ArrayAsList([|1 .. 10000000|])
let x_list = [1 .. 10000000]

#time

let rec decomposeList l cnt = 
    match l with
    | h :: t -> decomposeList t (cnt + 1)
    | [] -> cnt

decomposeList x_list 0
// Real: 00:00:00.017, CPU: 00:00:00.015, GC gen0: 0, gen1: 0, gen2: 0

let rec decomposeAAL l cnt = 
    match l with
    | UnconsArrayAsList(h, t) -> decomposeAAL t (cnt + 1)
    | EmptyArrayAsList -> cnt

decomposeAAL x_arr 0
// Real: 00:00:00.179, CPU: 00:00:00.171, GC gen0: 85, gen1: 1, gen2: 0

let rec decomposeAAL' (l: ArrayAsList<_>) cnt = 
    if l.IsEmpty then cnt
    else decomposeAAL' l.Tail (cnt + 1)

decomposeAAL' x_arr 0
// Real: 00:00:00.034, CPU: 00:00:00.031, GC gen0: 0, gen1: 0, gen2: 0

let rec decomposeAAL'' l cnt = 
    match l with
    | UnconsArrayAsList'(h, t) -> decomposeAAL'' t (cnt + 1)
    | EmptyArrayAsList' -> cnt
    | _ -> failwith "nope"

decomposeAAL'' x_arr 0
// Real: 00:00:00.183, CPU: 00:00:00.187, GC gen0: 86, gen1: 1, gen2: 0