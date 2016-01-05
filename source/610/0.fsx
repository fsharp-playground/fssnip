
//#r "FSharp.PowerPack.dll";;

open System.Collections.Generic

type t = HashMultiMap<int, unit -> unit>

let create () : t = new HashMultiMap<_, _>(13,HashIdentity.Structural)

let new_id : unit -> int =
    let id = ref 0 in
      fun () -> incr id; !id

let new_property () =
    let id = new_id () in
    let v = ref None in

    let set (t:t) x =
        t.Replace(id, (fun () -> v := Some x)) in

    let get (t:t) =
        try
            (t.[id]) ();
            match !v with
                | Some x as s -> v := None; s
                | None -> None
        with | :? KeyNotFoundException -> None
    in
    (set, get)

let get t (set, get) = get t
let set t (set, get) x = set t x 

//-- usage example 

let name = new_property ()
let age = new_property ()

let table = create ()

set table name "J.R. Hacker"
set table age 3

let _name = get table name
let _age  = get table age
