open System


let input = "
    ........
    ........
    ........
    ...BW...
    ...WB...
    ........
    ........
    ........
"
type Color = White | Black | None

let parseChar c = 
    match c with 
    | '.' -> None
    | 'W' -> White
    | 'B' -> Black
    | _ -> failwithf "unexpected chracter in input '%c'" c

  
let board = 
    input.Split('\n')
      |> Seq.map (fun s -> s.Trim())
      |> Seq.filter (fun s -> s <> "")
      |> Seq.map (Seq.map parseChar)
      |> array2D

type Board = Color[,]
let rec squaresInOneDirection (sx,sy) (px,py) = 
    if px < 0 || py < 0 || px >= 8 || py >= 8 then []
    else (px,py) :: squaresInOneDirection (sx,sy) (px+sx, py+sy)

//squaresInOneDirection (1,1) (2,3) = [(2, 3); (3, 4); (4, 5); (5, 6); (6, 7)]
let directions = [ for i in -1 .. 1 do for j in -1 .. 1 do if i <> 0 || j <> 0 then yield (i,j) ]
let squares = [ for i in 0 ..7 do for j in 0 .. 7 do yield (i,j) ]

let other c = match c with White -> Black | Black -> White | _ -> c

let isLegalMove c (px,py) = 
    board.[px,py] = None &&
    directions |> Seq.exists (fun dir -> 
        let squares = squaresInOneDirection dir (px,py)
        squares.Length > 1 &&
        let nx,ny = squares.[1]
        board.[nx,ny] = other c &&
        squares |> Seq.exists (fun (c2x,c2y) -> c = board.[c2x,c2y]))

let moves c = 
    squares |> List.filter (isLegalMove c) 

let blackMoves = moves  Black
let whiteMoves = moves White
