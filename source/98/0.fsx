module FSharp.Nullable

open System
open Microsoft.FSharp.Math

module Option =
    let fromNullable (n: _ Nullable) = 
        if n.HasValue
            then Some n.Value
            else None
    let toNullable =
        function
        | None -> Nullable()
        | Some x -> Nullable(x)

let (|Null|Value|) (x: _ Nullable) =
    if x.HasValue then Value x.Value else Null

module Nullable =
    let create x = Nullable x
    let getOrDefault n v = match n with Value x -> x | _ -> v
    let getOrElse (n: 'a Nullable) (v: 'a Lazy) = match n with Value x -> x | _ -> v.Force()
    let get (x: _ Nullable) = x.Value
    let fromOption = Option.toNullable
    let toOption = Option.fromNullable
    let bind f x =
        match x with
        | Null -> Nullable()
        | Value v -> f v
    let hasValue (x: _ Nullable) = x.HasValue
    let isNull (x: _ Nullable) = not x.HasValue
    let count (x: _ Nullable) = if x.HasValue then 1 else 0
    let fold f state x =
        match x with
        | Null -> state
        | Value v -> f state v
    let foldBack f x state =
        match x with
        | Null -> state
        | Value v -> f x state
    let exists p x =
        match x with
        | Null -> false
        | Value v -> p x
    let forall p x = 
        match x with
        | Null -> true
        | Value v -> p x
    let iter f x =
        match x with
        | Null -> ()
        | Value v -> f v
    let map f x =
        match x with
        | Null -> Nullable()
        | Value v -> f v
    let toArray x = 
        match x with
        | Null -> [||]
        | Value v -> [| v |]
    let toList x =
        match x with
        | Null -> []
        | Value v -> [v]
    
let liftNullable op (a: _ Nullable) (b: _ Nullable) =
    if a.HasValue && b.HasValue
        then Nullable(op a.Value b.Value)
        else Nullable()

let mapBoolOp op a b =
    match a,b with
    | Value x, Value y -> op x y
    | _ -> false

let inline (+?) a b = (liftNullable (+)) a b
let inline (-?) a b = (liftNullable (-)) a b
let inline ( *?) a b = (liftNullable ( *)) a b
let inline (/?) a b = (liftNullable (/)) a b
let inline (>?) a b = (mapBoolOp (>)) a b
let inline (>=?) a b = a >? b || a = b
let inline (<?) a b = (mapBoolOp (<)) a b
let inline (<=?) a b = a <? b || a = b
let inline notn (a: bool Nullable) = 
    if a.HasValue 
        then Nullable(not a.Value) 
        else Nullable()
let inline (&?) a b = 
    let rec and' a b = 
        match a,b with
        | Null, Value y when not y -> Nullable(false)
        | Null, Value y when y -> Nullable()
        | Null, Null -> Nullable()
        | Value x, Value y -> Nullable(x && y)
        | _ -> and' b a
    and' a b

let inline (|?) a b = notn ((notn a) &? (notn b))

type Int32 with
    member x.n = Nullable x

type Double with
    member x.n = Nullable x

type Single with
    member x.n = Nullable x

type Byte with
    member x.n = Nullable x

type Int64 with
    member x.n = Nullable x