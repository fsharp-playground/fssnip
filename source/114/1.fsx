//See examples:

//Input: ["a";"b"]
//Output: ["a"; "b"; "ab"]

//Input: ["a";"b";"c";"d"] 
//Output: ["a"; "b"; "c"; "d"; "ab"; "ac"; "ad"; "bc"; "bd"; "cd"; "bcd"; "abc"; "abd"; "acd"; "abcd"]

//Input: [1;2;5]
//Output: [1; 2; 5; 3; 6; 7; 8]

//Input: [2;3;8;12] 
//Output: [2; 3; 8; 12; 5; 10; 14; 11; 15; 20; 23; 13; 17; 22; 25]

//-----------------------------------------------------

let toDistinct mylist = (List.toSeq >> Seq.distinct >> Seq.toList) mylist;;

/// get combinations of sums
let rec recursivecombinations aggregate mylist = 
    match mylist with
    |[] -> []
    |h::t ->
        let tailpart = (toDistinct << recursivecombinations aggregate) t
        let headpart = t |> aggregate h
        let sums = headpart @ tailpart
        
        let recsums = match sums with
                        |[] -> []
                        |[x] -> [x]
                        | _ -> tailpart |> aggregate h
        sums @ recsums 

let combinations aggregate mylist = mylist @ recursivecombinations aggregate mylist |> toDistinct

//custom aggregate function, just sum here:
let inline sumfunction head = List.map (fun f ->head+f) 

//test:
//> combinations sumfunction ["a";"b";"c"];; 
//val it : string list = ["a"; "b"; "c"; "ab"; "ac"; "bc"; "abc"]

//> combinations sumfunction [1;2;3];; 
//val it : int list = [1; 2; 3; 4; 5; 6]
