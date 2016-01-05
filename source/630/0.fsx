(*[omit:Skip module definition on TryFSharp.org]*)
#if INTERACTIVE
#else
module Play
#endif
(*[/omit]*)

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Input
open System.Windows.Media
open System.Windows.Shapes

let width,height = 512,384
let move(shape,x,y) = Canvas.SetLeft(shape,float x); Canvas.SetTop(shape,float y)
let read(shape) = Canvas.GetLeft(shape) |> int, Canvas.GetTop(shape) |> int
let rectangle(x,y,w,h) =
    let shape= Rectangle(Width=float w,Height=float h,Fill=SolidColorBrush Colors.White)
    move(shape,x,y)
    shape
let run rate update =
    let rate = TimeSpan.FromSeconds(rate)
    let lastUpdate = ref DateTime.Now
    let residual = ref (TimeSpan())
    CompositionTarget.Rendering.Add (fun _ -> 
        let now = DateTime.Now
        residual := !residual + (now - !lastUpdate)
        while !residual > rate do
            update(); residual := !residual - rate
        lastUpdate := now
    )

type KeyState (control:Control) =
    let mutable keysDown = Set.empty  
    do  control.KeyDown.Add (fun e -> keysDown <- keysDown.Add e.Key)
    do  control.KeyUp.Add (fun e -> keysDown <- keysDown.Remove e.Key)        
    member this.IsKeyDown key = keysDown.Contains key

type Pad(keys:KeyState,up,down,x,y) =
    let shape = rectangle(x,y,10,60)
    let y = ref y
    member pad.Shape = shape
    member pad.Update () =
        if keys.IsKeyDown up then y := !y - 4
        if keys.IsKeyDown down then y := !y + 4
        move(shape,x,!y)

type Ball(blocks:Rectangle list) =
    let bx, by, bdx, bdy = ref (width/2), ref (height/4), ref 1, ref 1
    let shape = rectangle(!bx,!by,10,10)
    member ball.Shape = shape
    member ball.Update() =
        bx := !bx + !bdx*2
        by := !by + !bdy*2
        move(shape,!bx,!by)                       
        for block in blocks do
            let x,y = read block
            let w,h = int block.Width, int block.Height
            if !bx >= x && !bx < x + w && !by >= y && !by < y + h then
                if w > h then bdy := - !bdy else bdx := - !bdx 
                by := !by + !bdy*2; bx := !bx + !bdx*2

type GameControl() as control=
    inherit UserControl(Width=float width, Height=float height, IsTabStop=true)
    let keys = KeyState(control)
    let canvas = new Canvas(Background = SolidColorBrush Colors.Black)
    let top, bottom = rectangle(0,10,width,10), rectangle(0,height-20,width,10)
    let pad1, pad2 = Pad(keys,Key.Q,Key.A,10,60), Pad(keys,Key.P,Key.L,width-20,120)
    let ball = Ball([top;bottom;pad1.Shape;pad2.Shape])
    let (+.) (container:Panel) item = container.Children.Add item; container
    do  base.Content <- canvas+.top+.bottom+.pad1.Shape+.pad2.Shape+.ball.Shape
    let update () = pad1.Update(); pad2.Update(); ball.Update()
    do  async { 
        do! control.MouseLeftButtonDown |> Async.AwaitEvent |> Async.Ignore
        run (1.0/50.0) update 
        } |> Async.StartImmediate

(*[omit:Run script on TryFSharp.org]*)
#if INTERACTIVE
open Microsoft.TryFSharp
App.Dispatch (fun() -> 
    App.Console.ClearCanvas()
    let canvas = App.Console.Canvas
    let control = GameControl()    
    control |> canvas.Children.Add
    App.Console.CanvasPosition <- CanvasPosition.Right
    control.Focus() |> ignore
)
#endif
(*[/omit]*)