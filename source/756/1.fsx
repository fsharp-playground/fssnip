#if INTERACTIVE
#else
namespace MinesweeperGame
#endif

module Game =
    let compute (lines:string[]) =
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
                if y>=0 && y<lines.Length && x>=0 && x<lines.[y].Length
                then lines.[y].[x] |> value
                else 0
            )
        lines |> Array.mapi (fun y line ->
            line.ToCharArray() |> Array.mapi (fun x c ->
                let neighbours = count(x,y)    
                match c with
                | '*' -> c
                | '.' when neighbours > 0 -> '0' + char (neighbours)
                | '.' -> ' '
                | _ -> failwith "Unexpected value"  
            )
        )        

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

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media

type Minefield (lines:char[][]) as grid =
    inherit Grid()       
    do  for line in lines do grid.RowDefinitions.Add(RowDefinition()) 
        for _ in lines.[0] do grid.ColumnDefinitions.Add(ColumnDefinition())
    let squares =
        lines |> Array.mapi (fun y line ->         
            line |> Array.mapi (fun x c ->            
                let square = Button()            
                Grid.SetColumn(square,x)
                Grid.SetRow(square,y)            
                grid.Children.Add square
                c, square
            )       
        )
    let iter f =
        squares |> Array.iteri (fun y line ->
            line |> Array.iteri (fun x (c,square) ->f (x,y) (c,square))
        )
    let reveal (square:Button) c =
        square.Content <- c.ToString()
        square.Background <- SolidColorBrush Colors.Transparent
        square.Tag <- 1
    let isInRange (x,y) =
        y >= 0  && y < squares.Length &&
        x >= 0  && x < squares.[0].Length
    let canFill (x,y) = 
        isInRange (x,y) &&
        squares.[y].[x] |> fst |> ((=) ' ') &&
        ((squares.[y].[x] |> snd).Tag) = null
    let fill (x,y) = 
        if isInRange (x,y) then
            let c, square = squares.[y].[x] 
            reveal square c
    let mutable disposables = []
    let remember d = disposables <- d::disposables
    let listen (x,y) (c,square:Button) =
        square.Click |> Observable.subscribe (fun _ ->               
            if c = ' ' then flood canFill fill (x,y)  
            if c = '*' then reveal square '※'
            else reveal square c            
        ) |> remember
    do  iter listen
    interface IDisposable with
        member field.Dispose() =
            for d in disposables do d.Dispose()

type GameControl() =
    inherit UserControl(Width = 300.0, Height = 400.0)
    let minefield = "*...
....
.*..
...."
    let options = System.StringSplitOptions.RemoveEmptyEntries
    let lines = minefield.Split([|'\r';'\n'|], options)    
    let grid = new Minefield(Game.compute lines)
    do  base.Content <- grid

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