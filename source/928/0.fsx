
let input = """
  B.......
  ........
  ........
  ...BW...
  ...WB...
  ........
  ........
  ........"""

type color = | White | Black | Nothing

open System

let chartoColor s = 
    s |> Seq.map(function 
                   | 'W' -> White 
                   | 'B' -> Black
                   | '.' -> Nothing )
      |> Seq.toArray

let board = input.Split('\n') 
              |> Array.map(fun s -> s.Trim())
              |> Array.filter(fun s -> s <> "")
              |> Array.map chartoColor 
              |> array2D

let rec move (dx,dy) (x,y)  = seq{ 
    if not(x < 0 || y < 0 || x >= 8 || y >=8) then 
       yield (x,y), (board.[x,y])
       yield! move (dx,dy) (x+dx, y+dy) 
      }

let up = move (-1,0)
up (4,4)         



let otherColor = function
  | Black -> White
  | White -> Black
  | _ -> Nothing

let legalMove color seqmove = 
  let restlist= 
     seqmove 
      |> Seq.skip 1 
      |> Seq.skipWhile(fun ((x,y),c) -> c = otherColor color)
      |> Seq.toList

  match restlist, restlist.Length < (Seq.length seqmove)-1 with 
  | ((x,y), Nothing)::xs, true -> Some (x,y)
  | _ -> None

let directions = [(-1,0);(1,0);(0,-1);(0,1);(1,1);(-1,1);(1,-1);(-1,-1)] 

let moves color pos =
  directions |> List.choose (fun d -> move d pos |> legalMove color)
             |> Set.ofSeq

let generateMoves color = 
    seq {
      for x in 0 .. 7 do
        for y in 0 .. 7 do
          if color = board.[x,y] then
            yield moves color (x,y)
    }
    |> Seq.fold (Set.union) Set.empty


// ---------------------------------

[Black; White; White; Nothing ]
|> Seq.mapi (fun i c -> (0, i), c)
|> legalMove Black 

[Black; White; White ]
|> Seq.mapi (fun i c -> (0, i), c)
|> legalMove Black 

[Black; Nothing ]
|> Seq.mapi (fun i c -> (0, i), c)
|> legalMove Black 

[Black; White; Black; Nothing ]
|> Seq.mapi (fun i c -> (0, i), c)
|> legalMove Black 


moves Black (4, 4)
moves Black (3, 3)
moves Black (0, 0)  

generateMoves Black


