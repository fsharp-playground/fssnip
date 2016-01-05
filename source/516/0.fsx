let snoc p ls = ls @ [p]

let bump q L = 
    let rec loop P i = function
        | [] -> (None, snoc q P,i)
        | p::tail -> if (p<q) then loop (snoc p P) (i+1) tail
                              else (Some(p), P @ (q::tail),1) 
    loop [] 1 L;;

// Schensted insertion procedure
let insert q T =
    let rec loop q P i = function
        |[] -> snoc [q] P,(i,1)
        |p::tail -> let t = bump q p
                    match t with 
                    |None   ,L,x  -> (snoc L P) @ tail,(i,x)
                    |Some(r),L,_  -> loop r (snoc L P) (i+1) tail
    loop q [] 1 T ;;

let convert p =
    Seq.groupBy (fun ((x,_),_) -> x) p 
    |> Seq.toList
    |> List.sortBy (fun (a,_) -> a)
    |> List.map (fun (_,b) -> Seq.sortBy (fun ((_,y),_) -> y) b
                              |> Seq.toList 
                              |> List.map (fun (_,v) -> v))

let rsk L = 
    let rec loop Q P i = function
        |[] -> Q,P
        |p::tail -> 
                    let u,pos = insert p Q
                    loop u (snoc (pos,i) P) (i+1) tail
    let a,b = loop [] [] 1 L in a, convert b;;

let P1,Q1  = rsk [4;2;7;3;6;1;5];
let P2,Q2 = rsk [7;2;8;1;3;4;10;6;9;5];  

// The lenght of the longest rising subsequence in a permutation is equal
//to the lenght of the first row of its RSK-corresponding Young tableaux

let shape_of tableaux  = 
    let rec loop S = function
        |[] -> S
        |p::tail -> loop (snoc (List.length p) S) tail
    loop [] tableaux;;

let len = List.head (shape_of P2)