#if INTERACTIVE
#else
namespace MinesweeperGame
#endif

module Value =
    let count (chars:char[][]) (x,y) =
        [-1,-1; 0,-1; 1,-1
         -1, 0;       1, 0
         -1, 1; 0, 1; 1, 1]
        |> List.filter (fun (dx,dy) ->
            let x, y = x + dx, y + dy
            y>=0 && y<chars.Length && 
            x>=0 && x<chars.[y].Length &&
            chars.[y].[x] = '*'
        )
        |> List.length
    let from chars (x,y) c =
        let neighbours = count chars (x,y)
        match c with
        | '*' -> c
        | '.' when neighbours > 0 -> '0' + char (neighbours)
        | '.' -> ' '
        | _ -> new System.ArgumentException("Unexpected character") |> raise

[<AutoOpen>]
module Algorithm =
    let flood canFill fill (x,y) =
        let rec next = function
            | [] -> ()
            | ps ->
                let qs = 
                    ps 
                    |> List.filter canFill
                    |> List.collect (fun (x,y) -> 
                        [(x-1,y);(x+1,y);(x,y-1);(x,y+1)]
                    )
                ps |> List.iter fill
                qs |> next
        next [(x,y)]

type SquareState = { Value:char; mutable IsShowing:bool }
type SquareInfo = { X:int; Y:int; Value:char }

type Minefield (squares:SquareState[][]) =
    let height, width = squares.Length, squares.[0].Length
    let isInRange (x,y) =
        y >= 0  && y < height &&
        x >= 0  && x < width
    let surroundingSquares (x,y) =
        let values = System.Collections.Generic.List<_>()
        let canFill (x,y) = 
            isInRange (x,y) &&
            squares.[y].[x].Value=' ' &&
            not squares.[y].[x].IsShowing
        let fill (x,y) = 
            if isInRange (x,y) then
                let square = squares.[y].[x]
                square.IsShowing <- true
                values.Add(x,y,square.Value)
        flood canFill fill (x,y)
        values.ToArray()
    let mines =
        squares |> Array.mapi (fun y row -> 
            row |> Array.mapi (fun x square -> 
                (x,y,'*'), (square.Value = '*')
            )
        )
        |> Array.concat
        |> Array.filter snd
        |> Array.map fst
    let reveal (x,y) =
        let square = squares.[y].[x]
        match square.Value with
        | '*' -> mines
        | ' ' -> surroundingSquares (x,y)
        |  c  -> [|x,y,c|]
        |> Array.map (fun (x,y,c) -> { X=x; Y=y; Value=c })
    new (chars:char[][]) =
        Minefield(
            chars |> Array.mapi (fun y row ->
                row |> Array.mapi (fun x c -> 
                    let value = Value.from chars (x,y) c
                    { Value=value; IsShowing=false} )
            ))
    member field.Width = width
    member field.Height = height
    member field.Reveal(x,y) = reveal (x,y)

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media

type View (minefield:Minefield) as grid =
    inherit Grid()
    do  for i in 1..minefield.Height do
            grid.RowDefinitions.Add(RowDefinition()) 
        for i in 1..minefield.Width do 
            grid.ColumnDefinitions.Add(ColumnDefinition())
    let init f =
        [| for y in 0..minefield.Height-1 ->
            [| for x in 0..minefield.Width-1 -> f (x,y) |] |]
    let iteri f squares =
        squares |> Array.iteri (fun y line ->
            line |> Array.iteri (fun x square -> f (x,y) square)
        )
    let addSquare (x,y) =
        let square = Button()
        Grid.SetColumn(square,x)
        Grid.SetRow(square,y)
        grid.Children.Add square |> ignore
        square
    let squares = init addSquare
    let reveal (square:Button) c =
        square.Content <- c.ToString()
        square.Background <- SolidColorBrush Colors.Transparent
    let mutable disposables = []
    let remember d = disposables <- d::disposables
    let subscribe (x,y) (square:Button) =
        square.Click |> Observable.subscribe (fun _ ->
            let revealed = minefield.Reveal (x,y)
            for {X=x;Y=y;Value=c} in revealed do reveal (squares.[y].[x]) c
        ) |> remember
    do  iteri subscribe squares
    interface IDisposable with
        member field.Dispose() =
            for d in disposables do d.Dispose()

type GameControl() =
    inherit UserControl(Width = 300.0, Height = 400.0)
    let minefield = "*...
                     ....
                     .*..
                     ...."
    let chars = 
        let options = System.StringSplitOptions.RemoveEmptyEntries
        minefield.Split([|'\r';'\n'|], options)
        |> Array.map (fun line -> line.Trim().ToCharArray() )
    let minefield = Minefield(chars)
    let view = new View(minefield)
    do  base.Content <- view
    
#if INTERACTIVE
open Microsoft.TryFSharp
App.Dispatch (fun() -> 
    App.Console.ClearCanvas()
    let canvas = App.Console.Canvas
    canvas.Background <- SolidColorBrush Colors.Black
    let control = GameControl()    
    control |> canvas.Children.Add
    App.Console.CanvasPosition <- CanvasPosition.Right
    control.Focus() |> ignore
)
#else
type App() as app = 
    inherit Application()
    let main = new GameControl()
    do app.Startup.Add(fun _ -> app.RootVisual <- GameControl())
#endif