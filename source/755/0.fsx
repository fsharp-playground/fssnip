let board = "*...
....
.*..
...."

let compute (board:string[]) =
    let value c = 
        match c with
        | '*' -> 1
        | '.' -> 0
        | _ -> failwith "Unexpected value"
    let count (x,y) =
        [-1,-1; 0,-1; 1,-1
         -1, 0;       1, 0
         -1, 1; 0, 1; 1, 1]
        |> List.sumBy (fun (dx,dy) ->
            let x, y = x + dx, y + dy
            if y>=0 && y<board.Length && x>=0 && x<board.[y].Length
            then board.[y].[x] |> value
            else 0
        )
    board |> Array.mapi (fun y line ->
        line.ToCharArray() |> Array.mapi (fun x c ->         
            match c with
            | '*' -> c
            | '.' -> '0' + char (count(x,y))
            | _ -> failwith "Unexpected value"  
        ) |> fun xs -> (System.String(xs) |> string)
    )

let view =
    let options = System.StringSplitOptions.RemoveEmptyEntries
    let board = board.Split([|'\r';'\n'|], options) 
    compute board