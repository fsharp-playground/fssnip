open System
open System.Windows
open System.Windows.Controls
open System.Windows.Input
open System.Windows.Media

[<AutoOpen>]
module Mouse =
    let clickedOn (control:UIElement) =
        let down = control.MouseLeftButtonDown |> Event.map (fun e -> box e, 1)
        let up = control.MouseLeftButtonUp |> Event.map (fun e -> box e, -1)
        let mouseButton = Event.merge up down
        let mouseMove = control.MouseMove |> Event.map (fun e -> box e, 0)  
        let mouseEvents = Event.merge mouseButton mouseMove
        mouseEvents |> Event.scan (fun (_,last) (e,n) ->
           if last = 1 && n = -1 then Some(e),n 
           else None,n
        ) (None,0)
        |> Event.map (fun (e,_) -> 
            e |> Option.map (fun e -> e :?> MouseButtonEventArgs))
        |> Event.choose id
            
type AppControl() as control =
    inherit UserControl(Width = 320.0, Height = 200.0)
  
    let canvas = Canvas(Background = SolidColorBrush Colors.Orange)
    let block = TextBlock(Text="Hit Me", FontSize = 20.0)

    let mutable clicks = 0
    let clicked = Mouse.clickedOn control
    do  clicked.Add (fun _ -> 
          clicks <- clicks + 1
          block.Text <- "Still Clicks " + clicks.ToString()
        )
    
    do canvas.Children.Add(block)   
       base.Content <- canvas

#if INTERACTIVE
open Microsoft.TryFSharp
App.Dispatch (fun() -> 
    App.Console.ClearCanvas()
    AppControl() |> App.Console.Canvas.Children.Add
    App.Console.CanvasPosition <- CanvasPosition.Right
)
#endif