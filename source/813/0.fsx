let isEmpty = function
    | Empty -> true
    | _ -> false

let cons head tail= Cons(head,tail)

let head = function
    | Empty -> failwith "Source list is empty"
    | Cons(head,tail) -> head

let tail = function
    | Empty -> failwith "Source list is empty"
    | Cons(head,tail) -> tail

let rec (++) leftList rightList = 
    match leftList with
    | Empty -> rightList
    | Cons(head,tail) -> Cons(head,tail ++ rightList)

let rec update list index value =
    match (list,index,value) with
    | (Empty,_,_) -> failwith "Source list of empty"
    | (Cons(_,tail),0,v) -> Cons(v,tail)
    | (Cons(_,tail),i,v) -> update tail (i - 1) v