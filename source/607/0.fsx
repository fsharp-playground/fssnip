#r @"PresentationCore"
#r @"PresentationFramework"
#r @"WindowsBase"
#r @"System.Xaml"

open System.Windows
open System.Windows.Controls

let rect   = Shapes.Rectangle(Width=50.,Height=50., Fill=Media.Brushes.Red)
let canvas = Canvas()
canvas.Children.Add rect |> ignore
let window = Window(Width=200.,Height=200.)
window.Content <- canvas

let mutable offset = None
rect.MouseDown.Add <| fun e ->
  offset <- Some (e.GetPosition rect)
rect.MouseMove.Add <| fun e ->
  if offset.IsSome then
    let point = e.GetPosition canvas
    Canvas.SetLeft(rect, point.X - offset.Value.X)
    Canvas.SetTop (rect, point.Y - offset.Value.Y)
rect.MouseUp.Add <| fun _ -> offset <- None
rect.MouseLeave.Add <| fun _ -> offset <- None

window.Show()