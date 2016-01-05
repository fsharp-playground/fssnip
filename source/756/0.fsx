#if INTERACTIVE
#else
namespace MinesweeperGame
#endif

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media

module Game =
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
// [snippet:Show minesweeper board]
type GameControl() =
  inherit UserControl(Width = 300.0, Height = 400.0)
  let board = Game.view
  let grid = Grid()  
  do  board |> Array.iteri (fun y line ->
        grid.RowDefinitions.Add(RowDefinition())
        if y = 0 then
            for i = 1 to line.Length do
                grid.ColumnDefinitions.Add(ColumnDefinition())
        line.ToCharArray() |> Array.iteri (fun x c ->
            let button = Button()
            button.Content <- c.ToString()
            Grid.SetColumn(button,x)
            Grid.SetRow(button,y)
            grid.Children.Add button
        )
      )
  do  base.Content <- grid
// [/snippet]

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
