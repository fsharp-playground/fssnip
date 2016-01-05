let compute (board : string) =
    let options = System.StringSplitOptions.RemoveEmptyEntries
    let board = board.Split([| "\n" |], options)
    let count (x, y) =      
        [-1, -1; 0, -1;  1, -1;
         -1,  0;         1,  0;
         -1,  1; 0,  1;  1,  1]
         |> List.map (fun (x', y') -> x + x', y + y')
         |> List.filter (fun (x, y) -> 
            x >= 0 && x < board.[y].Length && 
            y >= 0 && y < board.Length
         )
         |> List.sumBy (fun (x, y) ->
            if board.[y].[x] = '*'
            then 1 else 0
         )
    board |> Array.mapi (fun y line ->
        line.ToCharArray() |> Array.mapi (fun x c ->
            match c with
            | '*' -> '*'
            | '.' -> '0' + char (count(x, y))
            | _ -> invalidArg "c" "Boo hiss!"
        )
    )
    |> Array.map (fun chars -> System.String(chars))
    |> Array.reduce (+)

let input = "*...
....
.*..
...."