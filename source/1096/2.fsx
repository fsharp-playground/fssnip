module BinPacking
#nowarn "25"

[<RequireQualifiedAccess>]
module Btd =

    //binary tree with duplicates: key-value, size, left subtree and right subtree
    type T<'a,'b> = T of ('a*'b)*int*T<'a,'b> option*T<'a,'b> option

    type private Direction = Left | Right
    type private Removed<'a,'b> = Removed of T<'a,'b> option | NotRemoved

    let empty = None

    let size t = match t with None -> 0 | Some (T(_,s,_,_)) -> s
    let private (~~) = size

    let toSeq t = 
        [Right,t] |> Seq.unfold (fun path ->
                match path with
                | [_,None] -> None // empty tree so end
                | (Right,Some(T(kv,_,_,None) as t))::rest -> Some(Some(kv),(Left,Some(t))::rest) //can't go right; yield and try to go left
                | (Right,Some(T(_,_,_,r)))::rest ->Some(None,(Right,r)::path) //go right as deep as possible
                | (Left,Some(T(_,_,None,_)))::[] -> None //can't go left and no parent so end
                | (Left,Some(T(_,_,None,_)))::(_,Some(T(kv,_,_,_) as t) )::rest -> Some(Some(kv),(Left,Some(t))::rest) //can't go left so go left at parent after yielding parent's value
                | (Left,Some(T(_,_,l,_)))::rest -> Some(None,(Right,l)::rest) //go right on the left child
                )
        |> Seq.choose (fun s -> s)

    let rec private percolate t path =
        match path with
        | [] -> t
        | (Left,T(kv,s,left,right))::rest ->  percolate (Some (T(kv,s - ~~left + ~~t, t, right))) rest
        | (Right,T(kv,s,left,right))::rest -> percolate (Some (T(kv,s - ~~right + ~~t, left, t))) rest

    let rec private insert (Some (T((ck,_),_,_,_)) as child) t path =
        match t with 
        | None -> percolate child path
        | Some (T((pk,_),_,l,r) as t2) ->
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
            | None -> NotRemoved
            | Some (T((k',v') as kv',_,l,r) as t') ->
                if kv = kv' then Removed(percolate (merge l r) path)
                elif k < k' then remove kv l ((Left,t')::path)
                else remove kv r ((Right,t')::path)
        match remove kv t [] with
        | NotRemoved -> t //failwithf "KV=%A  , T=%A" kv (t|>toSeq|>Seq.toArray)
        | Removed t -> t
    
    let findBestFit c t =
        let rec findBestFit c t currentBest =
            match t with
            | None -> currentBest
            | Some (T((k,v),_,l,r)) ->
                if c = k then Some(k,v)
                elif c < k then findBestFit c l (Some(k,v))
                else findBestFit c r currentBest
        findBestFit c t None

    let rec findWorstFit c t =
        match t with
        | None -> None
        | Some (T((k,v),_,_,None)) -> if k >= c then Some(k,v) else None
        | Some (T(_,_,_,r)) -> findWorstFit c r


type Bin<'a> = {Size:int; Id:string; Data:'a}
type Item<'a> = {Size:int; Data:'a} 

let rec private fillItems (map:Map<Bin<_>,Item<_>list>, t, remaining) fitFunc =
    match remaining with
    | [] -> map, t, []
    | x::rest ->
        match t |> fitFunc x.Size with
        | None -> map,t,remaining
        | Some ((capacity,bin) as kv) -> 
            let remainingCapacity = capacity - x.Size
            let t' = t |> Btd.remove kv |> Btd.add (remainingCapacity,bin)
            let map' = 
                match map |> Map.tryFind bin with
                | None -> map |> Map.add bin [x]
                | Some l -> map |> Map.add bin (x::l)
            fillItems (map', t', rest) fitFunc


///Best fit bin packing uses the tightest possible fit
///It fills one bin and then adds another if needed
let packBestFit (availableBins:Bin<_> list) (items:Item<_> list) =

    let scheduled,capacities,unscheduled = 
        ((Map.empty,Btd.empty,items), availableBins) ||> List.fold (fun (map,t,remaining) bin ->
            match remaining with 
            | [] -> map,t,[] //no more items remaining
            | xs -> fillItems (map,t |> Btd.add (bin.Size,bin),xs) Btd.findBestFit //add a bin to tree and fill items
         ) 
    scheduled |> Map.map (fun k v -> v |> List.rev), capacities, unscheduled


///Worst fit bin packing uses the loosest possible fit
///It spreads the load across all the bins evenly
let packWorstFit (bins:Bin<_> list) (items:Item<_> list) = 

    let binTree = (Btd.empty, bins) ||> List.fold (fun t b -> t |> Btd.add (b.Size,b))
    
    let scheduled,capacities,unscheduled =
        ((Map.empty,binTree,[]),items) ||> List.fold (fun (map,t,acc) item ->
            match t |> Btd.findWorstFit item.Size with
            | Some (capacity, bin) -> 
                let remainingCapacity = capacity - item.Size
                let t' = t |> Btd.remove (capacity, bin) 
                let t'' = t' |> Btd.add (remainingCapacity,bin)
                match map |> Map.tryFind bin with
                | Some xs -> map |> Map.add bin (item::xs), t'', acc
                | None -> map |> Map.add bin [item], t'', acc
            | None -> map, t, item::acc
            )
    scheduled |> Map.map (fun k v -> v |> List.rev), capacities, unscheduled


(* usage:

let bins = [{Size=150; Id="a"; Data="a"}; {Size=235; Id="b"; Data="b"}; {Size=215;Id="c"; Data="c"}]

let rng = System.Random()
let items = [for i in 1..40 -> {Size = rng.Next(1,30); Data=i.ToString()}]

//sort items in descending order of size for best-fit decreasing order
let sortedItems = items |> List.sortBy (fun i -> -i.Size)

let schBest,_,leftOverItemsA = packBestFit bins sortedItems
let schWorst,_,leftOverItemsB = packWorstFit bins sortedItems

*)
