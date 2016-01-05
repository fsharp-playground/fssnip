// Requires reference to 
// PresentationCore, PresentationFramework, 
// System.Windows.Presentation, System.Xaml, WindowsBase

open System
open System.Windows
open System.Windows.Media
open System.Windows.Shapes
open System.Windows.Controls

type Point = { X:float; Y:float }
type Triangle = { A:Point; B:Point; C:Point }

let transform (p1, p2, p3) =
   let x1 = p1.X + 0.5 * (p2.X - p1.X) + 0.5 * (p3.X - p1.X)
   let y1 = p1.Y + 0.5 * (p2.Y - p1.Y) + 0.5 * (p3.Y - p1.Y)
   let x2 = p1.X + 1.0 * (p2.X - p1.X) + 0.5 * (p3.X - p1.X)
   let y2 = p1.Y + 1.0 * (p2.Y - p1.Y) + 0.5 * (p3.Y - p1.Y)
   let x3 = p1.X + 0.5 * (p2.X - p1.X) + 1.0 * (p3.X - p1.X)
   let y3 = p1.Y + 0.5 * (p2.Y - p1.Y) + 1.0 * (p3.Y - p1.Y)
   { A = { X = x1; Y = y1 }; B = { X = x2; Y = y2 }; C= { X = x3; Y = y3 }}

let generateFrom triangle = seq {
      yield transform (triangle.A, triangle.B, triangle.C)
      yield transform (triangle.B, triangle.C, triangle.A)
      yield transform (triangle.C, triangle.A, triangle.B)
   }

let nextGeneration triangles =
   Seq.collect generateFrom triangles 
      
let render (target:Canvas) (brush:Brush) triangle =
   let points = new PointCollection()
   points.Add(new System.Windows.Point(triangle.A.X, triangle.A.Y))
   points.Add(new System.Windows.Point(triangle.B.X, triangle.B.Y))
   points.Add(new System.Windows.Point(triangle.C.X, triangle.C.Y))
   let polygon = new Polygon()
   polygon.Points <- points
   polygon.Fill <- brush
   target.Children.Add(polygon) |> ignore
   
let win = new Window()
let canvas = new Canvas()
canvas.Background <- Brushes.White
let brush = new SolidColorBrush(Colors.Black)
brush.Opacity <- 1.0
let renderTriangle = render canvas brush

let triangle = 
    let p1 = { X = 190.0; Y = 170.0 }
    let p2 = { X = 410.0; Y = 210.0}
    let p3 = { X = 220.0; Y = 360.0}
    { A = p1; B = p2; C = p3 }

let root = seq { yield triangle }
let generations = 
   Seq.unfold (fun state -> Some(state, (nextGeneration state))) root
   |> Seq.take 7
Seq.iter (fun gen -> Seq.iter renderTriangle gen) generations

win.Content <- canvas
win.Show()

[<STAThread()>]
do 
   let app =  new Application() in
   app.Run() |> ignore