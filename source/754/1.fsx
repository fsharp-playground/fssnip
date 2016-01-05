#if INTERACTIVE
#else
namespace Berzerk
#endif

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Input
open System.Windows.Media
open System.Windows.Media.Imaging

type Keys (control:Control) =
    let mutable keysDown = Set.empty
    do  control.KeyDown.Add (fun e -> keysDown <- keysDown.Add e.Key)
    do  control.KeyUp.Add (fun e -> keysDown <- keysDown.Remove e.Key)
    member keys.IsKeyDown key = keysDown.Contains key

[<AutoOpen>] 
module Imaging =
    let toInt (color:Color) = 
        (int color.A <<< 24) ||| 
        (int color.R <<< 16) ||| 
        (int color.G <<< 8)  ||| 
        (int color.B)
    let toBitmap color width (xs:int list) =
        let on = color |> toInt
        let off = Colors.Black |> toInt
        let toColor = function true -> on | false -> off
        let bitmap = WriteableBitmap(width, xs.Length)
        let pixels = bitmap.Pixels
        xs |> List.iteri (fun y xs ->
            for x = 0 to width-1 do
                let bit = 1 <<< (width - 1 - x) 
                pixels.[x+y*width] <- xs &&& bit = bit |> toColor
        )
        bitmap
    let toImage (bitmap:#BitmapSource) =
        let w = bitmap.GetValue(BitmapSource.PixelWidthProperty) :?> int
        let h = bitmap.GetValue(BitmapSource.PixelHeightProperty) :?> int
        Image(Source=bitmap,Stretch=Stretch.Fill,Width=float w,Height=float h) 
    let rotate xs =
        List.tail xs |> List.fold (fun ys xs ->
            List.zip xs ys
            |> List.map (fun (x,y) -> x::y)
        ) (List.head xs |> List.map (fun x -> [x]))
        |> List.map List.rev    

[<AutoOpen>]
module Game = 
    let rand = Random()
    let run rate update =
        let rate = TimeSpan.FromSeconds(rate)
        let lastUpdate = ref DateTime.Now
        let residual = ref (TimeSpan())
        CompositionTarget.Rendering.Subscribe (fun _ -> 
            let now = DateTime.Now
            residual := !residual + (now - !lastUpdate)
            while !residual > rate do
                update(); residual := !residual - rate
            lastUpdate := now
        )
    let move element (x,y) =
        Canvas.SetLeft(element, x)
        Canvas.SetTop(element, y)

[<AutoOpen>]
module Bits =
    let robot_bits = [ 
        0b00111100
        0b01100110
        0b11111111
        0b10111101
        0b10111101
        0b00111100
        0b00100100
        0b01100110
        ]

    let humanoid_bits = [
        [0b001100; 0b001100]
        [0b001100; 0b001100]
        [0b000000; 0b000000]
        [0b011111; 0b011111]
        [0b101100; 0b101100]
        [0b001100; 0b001100]
        [0b010010; 0b110100]
        [0b100010; 0b000100]
        ] 

type State = { mutable Image:Image; mutable X:float; mutable Y:float }

[<AutoOpen>]
module Robot =
    let random_pause state n = seq {
        let count = (rand.Next(10))
        for i = 1 to count do yield ()
    }

    let wait target state range = seq {
        let distance () = 
            let dx = (target.X - state.X)
            let dy = (target.Y - state.Y)
            sqrt(dx * dx + dy * dy) 
        while distance() > 50.0 do yield ()
    }

    let home target state n = seq {
        let dx = if target.X < state.X then -0.5 else 0.5
        let dy = if target.Y < state.Y then -0.5 else 0.5
        for i = 1 to n do          
            state.X <- state.X + dx
            state.Y <- state.Y + dy
            yield ()
        }

    let zombie target state = seq {
        yield! random_pause state 10
        while true do
            yield! wait target state 50.0
            yield! home target state 10
        }

type GameControl() as control =
    inherit UserControl(Width=200.0, Height=150.0,IsTabStop=true)
    let keys = Keys control
    let grid = Grid()
    let canvas = Canvas(Background = SolidColorBrush Colors.Black)
    do  grid.RenderTransform <- ScaleTransform(ScaleX=4.0,ScaleY=4.0)
    do  grid.Children.Add canvas
    do  control.Content <- grid
   
    let humanoids = 
        humanoid_bits |> rotate
        |> List.map (toBitmap Colors.Yellow 6 >> toImage)
    
    let humanoid = {Image=humanoids.[0]; X=control.Width/2.0; Y=control.Height/2.0}
    do  move humanoid.Image (humanoid.X,humanoid.Y)
    do  canvas.Children.Add humanoid.Image

    let robot = toBitmap Colors.Red 8 robot_bits
    let mutable robots = [
        for i = 1 to 5 do 
            let image = robot |> toImage
            let x = rand.Next(int control.Width - 8) |> float
            let y = rand.Next(int control.Height - 8) |> float
            move image (x,y)
            canvas.Children.Add(image)
            let state = { Image=image; X=x; Y=y }
            let machine = (zombie humanoid state)
            yield state, machine.GetEnumerator()
        ]

    let moveRobots () =
        robots <- robots |> List.filter (fun (state,machine) -> 
            let alive = machine.MoveNext()
            if alive then
                move state.Image (state.X, state.Y)
            alive
        )

    let update () =
        if keys.IsKeyDown Key.Up    then humanoid.Y <- humanoid.Y - 1.0
        if keys.IsKeyDown Key.Down  then humanoid.Y <- humanoid.Y + 1.0
        if keys.IsKeyDown Key.Left  then humanoid.X <- humanoid.X - 1.0
        if keys.IsKeyDown Key.Right then humanoid.X <- humanoid.X + 1.0
        move humanoid.Image (humanoid.X,humanoid.Y)       
        moveRobots ()

    let createMessage text =
        let t = TextBlock(Text=text, Foreground=SolidColorBrush Colors.White)
        t.HorizontalAlignment <- HorizontalAlignment.Center
        t.VerticalAlignment <- VerticalAlignment.Center
        t        

    let rec loop () = async {
        let t = createMessage "Click to Start"
        grid.Children.Add t
        do! control.MouseLeftButtonDown |> Async.AwaitEvent |> Async.Ignore
        grid.Children.Remove t |> ignore
        let _ = run (1.0/50.0) update
        do! Async.Sleep(-1)
        return! loop ()
        }
    do  loop () |> Async.StartImmediate

    let canvas = Canvas(Background = SolidColorBrush Colors.Black)
    do  canvas.RenderTransform <- ScaleTransform(ScaleX=4.0,ScaleY=4.0)

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