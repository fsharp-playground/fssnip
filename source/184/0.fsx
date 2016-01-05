let pascal n =
    //inner function that use tail-call recursion
    let rec _pascal accu x top =
        match x with
        | x when x = top -> accu
        | x -> let nv = let h = List.head accu 
                        //Lets code the 'visual' method using map2
                        List.map2 (+) (h @ [0]) (0 :: h) 
               _pascal (nv :: accu)  (x + 1) top
    _pascal [[1]] 1 n