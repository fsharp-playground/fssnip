let removeDuplicatesB(lst : 'a list) = 
    let f item acc =
        match acc with 
        | [] -> [item]
        | _ ->
            match List.exists(fun x -> x = item) acc with
            | false -> item :: acc
            | true -> acc
    List.foldBack f lst []
//  TEST BELOW NOTE: the list order is not maintained by the above snippit
//  As the order change is simply a feature of the foldBack process
//  the reversal of input and reversal of output is a reliable fix.  
["e"; "j"; "f"; "h"; "d"; "i"; "k"; "l"; "g"; "a"; "b"; "c"; "d"; "e"; "f"; "g" ] 
|> List.rev 
|>removeDuplicatesB 
|> List.rev
