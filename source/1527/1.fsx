let rec cast<'a> (myList: obj list) =          
    match myList with
    | head::tail -> 
        match head with 
        | :? 'a as a -> a::(cast tail) 
        | _ -> cast tail
    | [] -> [] 