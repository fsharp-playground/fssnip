// This file introduces our graphics functions
// We want to be able to:
// 1. Create a space with cartesian coordinate system, with given extents.
// 2. Be able to draw lines, text and dots on the space.
// 3. Be able to fill a bitmap with colors on the space.


// Import Try F# compatibility (enable for Visual Studio)
// #load "Offline.fsx"


////////////////////////////////////////
// Start Graphics //////////////////////
open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Media.Imaging
open Microsoft.TryFSharp

type Graphic = 
    { Canvas : Canvas;
      Size : float;
      Scale : float}
 
let makeCanvas (canvas : Canvas) size =
    let initSize = 250.0
    canvas.HorizontalAlignment <- HorizontalAlignment.Center
    canvas.VerticalAlignment <- VerticalAlignment.Center
    let scale = ScaleTransform()
    scale.ScaleX <- initSize / size
    scale.ScaleY <- -initSize / size
    canvas.RenderTransform <- scale
    { Canvas = canvas; Size = size; Scale = 2.0 * size / initSize}

let drawLine canvas brush (x1, y1) (x2, y2) = 
    let line = Shapes.Line()
    line.X1 <- x1
    line.Y1 <- y1
    line.X2 <- x2
    line.Y2 <- y2
    line.Stroke <- brush
    line.StrokeThickness <- 1.0 * canvas.Scale
    canvas.Canvas.Children.Add line |> ignore

let drawDot canvas fill (x, y) = 
    let dot = Shapes.Ellipse()
    dot.HorizontalAlignment <- HorizontalAlignment.Center
    dot.VerticalAlignment <- VerticalAlignment.Center
    dot.Height <- 2.0 * canvas.Scale
    dot.Width <- dot.Height
    dot.Fill <- fill
    Canvas.SetLeft(dot, x - dot.Width / 2.0)
    Canvas.SetTop(dot, y - dot.Width / 2.0)
    canvas.Canvas.Children.Add dot |> ignore

let drawDots canvas fill positions = 
    let grp = GeometryGroup()
    let addDot (x,y) = 
        let dot = EllipseGeometry()
        dot.Center <- Point(x,y)
        dot.RadiusX <- 0.1
        dot.RadiusY <- dot.RadiusX
        grp.Children.Add dot
    List.iter addDot positions
    let shape = Shapes.Path()
    shape.Data <- grp
    shape.Fill <- fill
    shape.HorizontalAlignment <- HorizontalAlignment.Center
    shape.VerticalAlignment <- VerticalAlignment.Center
    //Canvas.SetLeft(dot, x - dot.Width / 2.0)
    //Canvas.SetTop(dot, y - dot.Width / 2.0)
    canvas.Canvas.Children.Add shape |> ignore

let drawText canvas (x, y) text = 
    let tb = TextBlock()
    //tb.HorizontalAlignment <- HorizontalAlignment.Center
    //tb.VerticalAlignment <- VerticalAlignment.Center
    tb.Text <- text
    let scale = ScaleTransform()
    scale.ScaleX <- 1.0
    scale.ScaleY <- -1.0
    tb.RenderTransform <- scale
    tb.FontSize <- 10.0 * canvas.Scale
    Canvas.SetLeft(tb, x)
    Canvas.SetTop(tb, y)
    canvas.Canvas.Children.Add tb |> ignore

let drawAxes canvas tickUnit =
    let darkGray = SolidColorBrush(Colors.DarkGray)
    drawLine canvas darkGray (-canvas.Size, 0.0) (canvas.Size, 0.0)
    drawLine canvas darkGray (0.0, -canvas.Size) (0.0, canvas.Size)
    let ticks = [0.0 .. tickUnit .. canvas.Size] @ [-tickUnit .. -tickUnit .. -canvas.Size]
    let tickSize = tickUnit / 10.0
    List.iter (fun x -> drawLine canvas darkGray (x, -tickSize) (x, tickSize)) ticks
    List.iter (fun y -> drawLine canvas darkGray (-tickSize, y) (tickSize, y)) ticks
    
let drawBitmap canvas colorFunc =
    let getCoordinates =
        let step = canvas.Size * 2.0 / 100.0
        [ for x in -canvas.Size .. step .. canvas.Size do
            for y in -canvas.Size .. step .. canvas.Size do
                yield (x, y) ]
    List.iter (fun p -> drawDot canvas (colorFunc p) p) getCoordinates

let drawTestDot canvas fill =
    drawDot canvas fill (-canvas.Size, canvas.Size)
    // Draw 100 * 100 dots over the canvas size
//
//
//let drawBitmap canvas colorFunc =
//    let setPixel (bm:WriteableBitmap) row col (alpha : byte) (r : byte) (g : byte) (b : byte) = 
//        let idx = row * bm.PixelWidth + col
//        let r1 = byte ((uint16 r) * (uint16 alpha) / (uint16 255))
//        let g1 = byte ((uint16 g) * (uint16 alpha) / (uint16 255))
//        let b1 = byte ((uint16 b) * (uint16 alpha) / (uint16 255))
//        Array.set bm.Pixels idx (((((int alpha) <<< 24) ||| ( int r1 <<< 16)) ||| (int g1 <<< 8)) ||| int b1)
//    let draw (bm:WriteableBitmap) x =
//        for i = 0 to 499 do
//            for j = 0 to 499 do
//                setPixel bm i j 255uy (byte i) (byte j) (200uy + x)
//        bm.Invalidate()
//    let bm = new WriteableBitmap(500, 500, 96.0, 96.0, PixelFormats.Bgr32, null )
//    let image = new Image()
//    image.Source <- bm
//    canvas.Canvas.Children.Add image

let draw () =
    let canvas = makeCanvas App.Console.Canvas 10.0
    let black = SolidColorBrush(Colors.Black)
    let red = SolidColorBrush(Colors.Red)
    let green = SolidColorBrush(Colors.Green)
    let blue = SolidColorBrush(Colors.Blue)
    let brown = SolidColorBrush(Colors.Brown)
    // drawBitmap canvas (fun (x,y) -> if x > 0.0 then red else blue)
    drawDots canvas brown [(-5.0, 3.0); (-4.0, 3.0)]
    drawAxes canvas 1.0
    drawText canvas (4.0, -1.0) "Hello"
    drawLine canvas black (1.0, 1.0) (10.0, 1.0)
    drawLine canvas red (-8.0,-8.0) (0.0, 0.0)
    drawDot canvas green (2.0, 3.0)  
    drawTestDot canvas blue
    List.iter (drawDot canvas red) [for x in -8.0..0.2..8.0 -> (x, 4.0)]  
    App.Console.CanvasPosition <- CanvasPosition.Alone

App.Dispatch (fun() -> draw())
