open System
open System.Windows
open System.Windows.Controls
open System.Windows.Input
open System.Windows.Media

[<AutoOpen>]
module Mouse =
    type evt =
        | Down
        | Up
        | Move

    let clickedOn (control:UIElement) =
        let down = control.MouseLeftButtonDown |> Event.map (fun _ -> Down)
        let up = control.MouseLeftButtonUp |> Event.map (fun _ -> Up)
        let move = control.MouseMove |> Event.map (fun _ -> Move)  
        let mouseEvents = Event.merge move (Event.merge up down)
        let click =
            mouseEvents
            |> Event.scan (fun (lastEvt,_) newEvt ->
                match lastEvt, newEvt with
                | Some Down, Up -> Some Up, Some()
                | _, x -> Some x, None    
            ) (None, None)
            |> Event.choose snd
        click
    
type AppControl() as control =
  inherit UserControl(Width = 320.0, Height = 200.0)
  
  let canvas = Canvas(Background = SolidColorBrush Colors.Orange)
  let block = TextBlock(Text="Hit Me2", FontSize = 20.0)

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