// Takes an input like [[1;2;5];[3;4];[6;7]] and returns
// [[5; 3; 7]; [2; 3; 7]; [1; 3; 7]; [5; 4; 7]; [2; 4; 7];
// [1; 4; 7]; [5; 3; 6]; [2; 3; 6]; [1; 3; 6]; [5; 4; 6];
// [2; 4; 6]; [1; 4; 6]]

let rec cartesian lstlst =
    match lstlst with
    | h::[] ->
        List.fold (fun acc elem -> [elem]::acc) [] h
    | h::t ->
        List.fold (fun cacc celem ->
            (List.fold (fun acc elem -> (elem::celem)::acc) [] h) @ cacc
            ) [] (cartesian t)
    | _ -> []
