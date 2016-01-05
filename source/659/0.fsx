(* Transposes rectangular matrices *)
let transpose matrix =
    let rec fetch_column acc (matr:(int list list)) = (* Makes a column list from a row list *)
        if matr.Head.Length = 0 then (List.rev acc) (* Stop *)
        else fetch_column
                ([for row in matr -> row.Head]::acc) (* Fetches the first item from each row *)
                (List.map (fun row -> match row with [] -> [] | h::t -> t) matr)
    fetch_column [] matrix

transpose [[1;2;3;4];[5;6;7;8];[9;10;11;12]]