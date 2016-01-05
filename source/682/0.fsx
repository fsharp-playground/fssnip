type Color = 
    Black | White
    with member x.Invert() = match x with | Black -> White | White -> Black
type Direction = 
    Up | Down | Left | Right
    with 
    member x.ToRight() = match x with | Up -> Right | Right -> Down | Down -> Left | Left -> Up
    member x.ToLeft() =  match x with | Up -> Left | Right -> Up | Down -> Right | Left -> Down
type Position =
    { X : int; Y : int }
    with 
    member this.Move(direction) =
        match direction with
        | Up    -> {this with Y=this.Y+1}
        | Right -> {this with X=this.X+1}
        | Down  -> {this with Y=this.Y-1}
        | Left  -> {this with X=this.X-1}
    member this.ToTup() = (this.X,this.Y)
type Field = {
    Pos : Position
    Color : Color
    }
    
type Ant = {
    Pos : Position
    Direction : Direction
    }
    with 
    member x.Move(field:Field) = 
        let newDirection = match field.Color with
                           | White -> x.Direction.ToRight()
                           | Black -> x.Direction.ToLeft()
        let newPosition =  x.Pos.Move(newDirection)
        { Direction = newDirection
          Pos = newPosition }       // return new Ant
  
let makeStep (ant:Ant) (fields:Map<int*int,Field>) =
    let position = ant.Pos.ToTup()
    let currentField = match fields.TryFind(position) with
                       | Some(f) -> f
                       | None    -> { Pos = ant.Pos; Color = White }

    let invertedField = { currentField with Color = currentField.Color.Invert() }
    ant.Move(currentField), invertedField
let rec antRoute ant fields = 
    seq {
        let newAnt, invertedField = makeStep ant fields
        yield invertedField
        yield! antRoute newAnt (fields.Add(ant.Pos.ToTup(), invertedField))
    }

let ant = { Pos = {X=0; Y=0;}; Direction = Up }
antRoute ant Map.empty
    |> Seq.take 1000 
    |> Seq.filter (fun f -> f.Color = Black)
    |> Seq.toList
    |> List.sortBy (fun f -> f.Pos)
    |> List.iter (fun f -> printfn "[%d,%d]-%A" f.Pos.X f.Pos.Y f.Color)