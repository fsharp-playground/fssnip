#if INTERACTIVE
open Microsoft.TryFSharp
#else
namespace Ag
#endif

open System
open System.Windows
open System.Windows.Controls
// [snippet:WrapPanel control]
type WrapPanel() =
    inherit Panel()
    override x.MeasureOverride(availableSize:Size) =
        let childMeasure = Size(Double.PositiveInfinity, Double.PositiveInfinity)
        let _, (width,height) =
            x.Children 
            |> Seq.fold (fun ((x, y), (width, height)) child ->
                child.Measure(childMeasure)
                if child.DesiredSize.Width + x > availableSize.Width 
                then (child.DesiredSize.Width, 0.0), (max x width, height + y)
                else (x + child.DesiredSize.Width, y), (width, height)
            ) ((0.0,0.0),(0.0,0.0))
        Size(width, height)
    override x.ArrangeOverride(finalSize:Size) =
        x.Children 
        |> Seq.fold (fun (x,y,height) child ->
            let x,y,height =
                if child.DesiredSize.Width + x > finalSize.Width 
                then 0.0, y + height, 0.0
                else x, y, height
            Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height)
            |> child.Arrange
            x + child.DesiredSize.Width, y, max height child.DesiredSize.Height
        ) (0.0,0.0,0.0) 
        |> ignore
        finalSize
// [/snippet]
open System.Windows.Media

// [snippet:Sample usage]
type NumberBox(number:int) as this =
    inherit UserControl(Width=50.0,Height=50.0)
    let block = 
        TextBlock(Text=number.ToString(),
                  Foreground=SolidColorBrush Colors.Blue,
                  HorizontalAlignment=HorizontalAlignment.Center,
                  VerticalAlignment=VerticalAlignment.Center,
                  FontSize=16.0)
    let border = 
        Border(Child=block,
               BorderBrush=SolidColorBrush Colors.Cyan,
               BorderThickness=Thickness(2.0),
               Margin=Thickness(5.0))
    do  this.Content <- border
// [/snippet]

#if INTERACTIVE
// [snippet:Run sample in Try F#]
App.Dispatch (fun () ->
    let panel = WrapPanel(Width=400.0)
    for i = 1 to 50 do NumberBox i |> panel.Children.Add
    App.Console.ClearCanvas()
    let canvas = App.Console.Canvas
    panel |> canvas.Children.Add
    App.Console.CanvasPosition <- CanvasPosition.Right
)
// [/snippet]
#else
type App() as app =
  inherit System.Windows.Application()
  let panel = WrapPanel()
  do for i = 1 to 50 do NumberBox i |> panel.Children.Add
  do  app.Startup.Add(fun _ -> app.RootVisual <- panel)
#endif