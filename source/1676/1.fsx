let crossJoin xss = 
    let rec outerListLoop acc xs =
        match xs with
        | [] -> acc
        | x::rest -> innerListLoop acc x rest
    and innerListLoop acc ys xs =
        match ys with
        | []    -> acc
        | y::rest -> 
            let acc' = (y::List.head acc)::List.tail acc
            let os = outerListLoop acc' xs
            let is = innerListLoop acc rest xs 
            os @ if rest |> List.isEmpty then List.tail is else is
    let xss' = xss |> List.filter (fun xs -> List.length xs > 0)
    outerListLoop [[]] xss'


(*
let data = [[1;2];[3;7];[4;5;6]] 
crossJoin data
val it : int list list =
  [[4; 3; 1]; [5; 3; 1]; [6; 3; 1]; [4; 7; 1]; [5; 7; 1]; [6; 7; 1]; [4; 3; 2];
   [5; 3; 2]; [6; 3; 2]; [4; 7; 2]; [5; 7; 2]; [6; 7; 2]]
*)
