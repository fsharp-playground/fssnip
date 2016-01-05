type Color =  Black | White
type Direction =  Up | Down | Left | Right
type Position = { X : int; Y : int }
type Field = { Pos : Position; Color : Color }
type Ant = {  Pos : Position; Direction : Direction }

module Utils = 
    let toTuple position = (position.X, position.Y)    
    let fromTuple position = { X = fst position; Y = snd position }
    let toLeft       = function | Up -> Left | Right -> Up | Down -> Right | Left -> Down
    let toRight      = function | Up -> Right | Right -> Down | Down -> Left | Left -> Up
    let invert = function
        | f when f.Color = Black -> {f with Color=White }
        | f                      -> {f with Color=Black }
    let move position = function
        | Up    -> {position with Y=position.Y+1}
        | Right -> {position with X=position.X+1}
        | Down  -> {position with Y=position.Y-1}
        | Left  -> {position with X=position.X-1}

module Board = 
    open Utils
    let getColor field = field.Color
    let getField (fields:Map<int*int,Field>) position =
        match fields.TryFind(position) with
        | Some(f) -> f
        | None    -> { Pos = fromTuple position; Color = White }
    let getNewDirection ant color = 
        let fn = match color with 
                 |White -> toRight
                 |Black -> toLeft
        fn ant.Direction
    let move ant newDirection =
        { Direction = newDirection; Pos = move ant.Pos newDirection },      // new ant
        ant                                                             // old ant
open Utils
open Board
    
let makeStep (ant:Ant) (fields:Map<int*int,Field>) =
    toTuple ant.Pos
    |> getField fields
    |> getColor
    |> getNewDirection ant
    |> move ant

let rec antRoute ant fields = 
    seq {
        let newAnt, oldPosition = match makeStep ant fields with
                                  | newAnt, oldAnt -> newAnt, (toTuple oldAnt.Pos)
        let invertedField = oldPosition |> getField fields |> invert
        yield invertedField
        yield! antRoute newAnt (fields.Add(oldPosition, invertedField))
    }

// go
let ant = { Pos = {X=0; Y=0;}; Direction = Up }
antRoute ant Map.empty
    |> Seq.take 1000 
    |> Seq.filter (fun f -> f.Color = Black)
    |> Seq.toList
    |> List.sortBy (fun f -> f.Pos)
    |> List.iter (fun f -> printfn "[%d,%d]-%A" f.Pos.X f.Pos.Y f.Color)