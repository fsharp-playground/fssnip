let boardText ="
........
........
........
...BW...
...WB...
........
........
........"
let board = boardText.Split([|'\r';'\n'|]) |> Array.filter (fun line -> line.Length > 0)

let getLegalMoves (board:string[]) color =
    let width, height = board.[0].Length, board.Length
    let opposite =
        match color with
        | 'B' -> 'W'
        | 'W' -> 'B'
        | _ -> invalidOp ""
    let findDisks color =
        board |> Seq.mapi (fun y line ->
            line |> Seq.mapi (fun x item -> if item = color then Some(x,y) else None)
        )                 
        |> Seq.concat
        |> Seq.choose id
    let directions =
        [-1,-1; 0,-1; 1, -1;
         -1, 0;       1, 0;
         -1, 1; 0, 1; 1, 1]
    let isInside (x,y) = x >= 0 && x < width && y >= 0 && y < height
    let itemAt (x,y) = board.[y].[x]
    let findSurroundingPlaces (x,y) =
        directions
        |> List.map (fun (dx,dy) -> x+dx, y+dy)
        |> List.filter isInside        
    let isLegalMove (x,y) =
        directions
        |> List.filter (fun (dx,dy) -> isInside (x+dy,y+dy))
        |> List.filter (fun (dx,dy) -> opposite = itemAt (x+dx,y+dy)) 
        |> List.map (fun (dx,dy) ->           
            let xs = 
                Seq.initInfinite (fun i -> (x+dx*i, y+dy*i))
                |> Seq.skip 1
                |> Seq.takeWhile isInside
                |> Seq.skipWhile (fun (x,y) -> itemAt(x,y) = opposite)
            if Seq.length xs = 0 then false
            else Seq.head xs |> itemAt = color
        )
        |> List.exists id
    let targets = findDisks opposite
    targets |> Seq.map (fun (x,y) ->
        findSurroundingPlaces (x,y)
        |> List.filter (fun (x,y) -> board.[y].[x] = '.')
        |> List.filter isLegalMove
    )
    |> Seq.concat

getLegalMoves board 'B'