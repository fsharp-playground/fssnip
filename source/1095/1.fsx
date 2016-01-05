module BinPacking

[<RequireQualifiedAccess>]
module Btd =

    //binary tree with duplicates: key-value, size, left subtree and right subtree
    type T<'a,'b> = T of ('a*'b)*int*T<'a,'b> option*T<'a,'b> option

    type private Direction = Left | Right

    let empty = None

    let size t = match t with None -> 0 | Some (T(_,s,_,_)) -> s
    let private (~~) = size

    let rec private percolate t path =
        match path with
        | [] -> t
        | (Left,T(kv,s,left,right))::rest ->
             percolate (Some (T(kv,s - ~~left + ~~t, t, right))) rest
        | (Right,T(kv,s,left,right))::rest -> percolate (Some (T(kv,s - ~~right + ~~t, left, t))) rest

    let rec private insert child t path =
        match t with 
        | None -> percolate child path
        | Some (T((pk,_),_,l,r) as t2) ->
            match child with
            | None -> t
            | Some (T((ck,_),_,_,_)) ->
                if ck < pk then insert child l ((Left,t2)::path)
                else insert child r ((Right,t2)::path)

    let add e t = insert (Some(T(e,1,None,None))) t []

    let merge l r =
        match l,r with
        | None, t -> t
        | t, None -> t
        | Some(T(_,ls,_,_)),Some(T(_,rs,_,_)) ->
            if ls < rs then insert r l []
            else insert l r []
            
    let remove kv t =
        let rec remove ((k,v) as kv) t path =
            match t with
            | None -> None
            | Some (T((k',v') as kv',s,l,r) as t) ->
                if kv = kv' then 
                    match path with 
                    | [] -> merge l r //root removed
                    | _  -> percolate None path
                elif k < k' then remove kv l ((Left,t)::path)
                else remove kv r ((Right,t)::path)
        match remove kv t [] with
        | None -> t
        | x -> x
    
    let findBestFit c t =
        let rec findBestFit c t currentBest =
            match t with
            | None -> currentBest
            | Some (T((k,v),_,l,r)) ->
                if c = k then Some(k,v)
                elif c < k then findBestFit c l (Some(k,v))
                else findBestFit c r currentBest
        findBestFit c t None

type Bin<'a> = {Size:int; Id:string; Data:'a}
type Item<'a> = {Size:int; Data:'a} 

let rec private fillItems (map:Map<Bin<_>,Item<_>list>, t, remaining) =
    match remaining with
    | [] -> map, t, []
    | x::rest ->
        match t |> Btd.findBestFit x.Size with
        | None -> map,t,remaining //can't add any more items into the current bin tree structure
        | Some ((capacity,bin) as kv) -> 
            let remainingCapacity = capacity - x.Size
            let t' = t |> Btd.remove kv |> Btd.add (remainingCapacity,bin)
            let map' = 
                match map |> Map.tryFind bin with
                | None -> map |> Map.add bin [x]
                | Some l -> map |> Map.add bin (x::l)
            fillItems (map', t', rest)

///implements the best-fit heuristic algorithm for 1-dimensional bin packing
let pack (availableBins:Bin<_> list) (items:Item<_> list) =
    let map,t,remaining = 
        ((Map.empty,Btd.empty,items), availableBins) ||> List.fold (fun (map,t,remaining) bin ->
            match remaining with 
            | [] -> map,t,[] //no more items remaining
            | xs -> fillItems (map,t |> Btd.add (bin.Size,bin),xs)) //add a bin to tree and fill items
    map,remaining

(* usage:

let bins = [{Size=150; Id="a"; Data="a"}; {Size=235; Id="b"; Data="b"}; {Size=215;Id="c"; Data="c"}]

let rng = System.Random()
let items = [for i in 1..40 -> {Size = rng.Next(1,30); Data=i.ToString()}]

//sort items in descending order of size for best-fit decreasing order
let sortedItems = items |> List.sortBy (fun i -> -i.Size)

let schedule,leftOverItems = pack bins sortedItems

*)
