// -- | Auxiliary recursive drop function
let rec drop' i l1 p l2 =
    if List.length l2 = List.length l1 - 1 then l2
    else 
        let p = if p = i then p+1  else p 
        drop' i l1 (p+1) (List.append l2 [(List.nth l1 p)])

// -- | Removes the the nth element from list
let drop i l =
    drop' i l 0 [] 

// -- | Use cases
let list = [1..10] 
printfn "%A" drop 0 list
printfn "%A" drop 1 list
printfn "%A" drop (List.length list) list