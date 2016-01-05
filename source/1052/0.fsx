open System

/// Reversi board
let board = "
....
.OX.
.XO.
...."

/// Board rows
let rows = board.Split([|'\n'|], StringSplitOptions.RemoveEmptyEntries)

/// Board cells
let cells = 
    rows 
    |> Array.map (fun row -> row.ToCharArray())
    |> array2D

/// Returns true if coordinates are inside board
let inside (x,y) = 
    x >=0 && x < rows.[0].Length && y >= 0 && y < rows.Length 

/// Gets vectors from a coordinate
let getVectors (x,y) =
    [-1,-1; 0,-1; 1,-1
     -1, 0;       1, 0
     -1, 1; 0, 1; 1, 1]
    |> List.filter (fun (dx,dy) -> inside(x+dx,y+dy)) 

/// Returns true if board run represents a valid move
let isValidRun (s:string) =
    let color = s.[0]
    let opposite =
        match color with
        | 'O' -> 'X'
        | 'X' -> 'O'
        |  c  -> invalidOp(sprintf "Unexpected char %c" c)
    let mutable i = 1
    while i < s.Length && s.[i] = opposite do i <- i + 1
    i > 1 && i < s.Length && s.[i] = color

/// Gets board run as string
let getRun (x,y) (dx,dy) =
    let px = ref (x+dx)
    let py = ref (y+dy)
    let chars =
        [|while inside(!px,!py) do 
            yield cells.[!py,!px]
            px := !px + dx
            py := !py + dy|]
    String(chars)

/// Prints all valid moves for a color
let validMoves (color:string) =
    cells 
    |> Array2D.iteri (fun y x cell ->
        if cell = '.' then
            getVectors (x,y)
            |> List.exists (fun vector ->
                let s = getRun (x,y) vector
                isValidRun (color+s)
            )
            |> function 
                | true -> printfn "%d %d" x y 
                | false -> ()
    )