type 'a Tree = Node of 'a * seq<'a Tree> * seq<'a Tree>

let addL a t = 
    match t with
    | Node (v,l,r) -> Node (v,seq {yield! l; yield a},r)

let addR a t = 
    match t with
    | Node (v,l,r) -> Node (v,l,seq {yield! r; yield a})

let removeL a t = 
    match t with
    | Node (v,l,r) -> Node (v,Seq.filter (fun i -> i = a) l ,r)

let removeR a t = 
    match t with
    | Node (v,l,r) -> Node (v,l , Seq.filter (fun i -> i = a) r)

let newTree v = Node (v,Seq.empty,Seq.empty)

let tree = newTree 1 |> addL (newTree 2) |> addR (newTree 3)