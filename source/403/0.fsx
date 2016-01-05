open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Shapes
open Microsoft.TryFSharp


let line (x1, y1) (x2, y2) =
    App.Dispatch(fun () ->
        let s = new Line(X1=x1, Y1=y1, X2=x2, Y2=y2)
        s.Stroke <- new SolidColorBrush(Colors.Black)
        App.Console.Canvas.Children.Add s
    ) |> ignore

let rectangle (x1, y1) (x2, y2) (x3, y3) (x4, y4) =
    line (x1, y1) (x2, y2)
    line (x2, y2) (x3, y3)
    line (x3, y3) (x4, y4)
    line (x4, y4) (x1, y1)

rectangle (20.0, 200.0) (20.0, 400.0) (200.0, 400.0) (200.0, 200.0)
