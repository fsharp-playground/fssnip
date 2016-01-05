// Part 1. - Complex Basics
// Part 2. - Graphics Basics
// Part 3. - Drawing Complex Sets

// Import Try F# compatibility (enable for Visual Studio)
// #load "Offline.fsx"

////////////////////////////////////////
// Part 1. - Complex Basics

// Define complex type with some operators
type Complex =
    { Re : float;
      Im : float }
    static member (+) (z1, z2) = 
        { Re = z1.Re + z2.Re; 
          Im = z1.Im + z2.Im }
    static member (-) (z1, z2) = 
        { Re = z1.Re - z2.Re; 
          Im = z1.Im - z2.Im }
    static member (*) (z1, z2) = 
        { Re = ((z1.Re * z2.Re) - (z1.Im * z2.Im));
          Im = ((z1.Re * z2.Im) + (z1.Im * z2.Re)) }
    static member (/) (z1, z2) = 
        let z2_conj = {Re = z2.Re; Im = -z2.Im}
        let den = (z2 * z2_conj).Re
        let num = z1 * z2_conj
        { Re = num.Re / den;
          Im = num.Im / den }
    static member (~-) z = 
        { Re = -z.Re; 
          Im = -z.Im };;

// .. and printing
let print z = printfn "%.3f%+.3fi" z.Re z.Im;;
let sprint z = sprintf "%.3f%+.3fi" z.Re z.Im;;

// .. and the conjugate
let conj z = 
    { Re = z.Re; 
      Im = -z.Im };;

// ... and the modulus (absolute value)
let abs z =
    sqrt (z.Re * z.Re + z.Im * z.Im);;

// ... and the argument (actually this is the principal value of the argument (Arg)
let arg z = 
    atan2 z.Im z.Re;;

// Polar form of complex number
type ComplexPolar = 
    { Mag : float;
      Arg : float };;

// ... with conversion to and from the polar form
let toPolar z = 
    { Mag = abs z;
      Arg = arg z };;

let fromPolar zp = 
    { Re = zp.Mag * (cos zp.Arg);
      Im = zp.Mag * (sin zp.Arg) };;

// ... and define printing of the polar form
let printp zp = 
    printfn "%.1f(cos %.3f + i sin %.3f)" zp.Mag zp.Arg zp.Arg;;

// Get list of angles used for roots
let rootAngles theta n =
    let pi = atan2 0.0 -1.0
    let kList = [0 .. (n-1)]
    let angles = List.map (fun k -> (theta + 2.0 * (float k) * pi) / (float n)) kList
    let anglesModPi = List.map (fun angle -> angle % (2.0 * pi)) angles
    let anglesSorted = List.sort anglesModPi
    anglesSorted;;

// Find roots
let nthRootsPolar n z = 
    let zp = toPolar z
    let angles = rootAngles zp.Arg n
    let mag = System.Math.Pow(zp.Mag, (1.0 / (float n)))
    List.map (fun angle -> {Mag = mag; Arg = angle}) angles;;

// ... one way to convert a list from polar
let fromPolarList polars = 
    List.map fromPolar polars;;

// ... another way...
// send (pipe) the output from the nthRootsPolar 
// to a list conversion
let nthRoots n z = 
    nthRootsPolar n z
    |> List.map fromPolar;;


// Define some standard complex numbers
let zero = {Re = 0.0; Im = 0.0}
let one = {Re=1.0; Im = 0.0}
let i = {Re=0.0; Im = 1.0}

////////////////////////////////////////
// Part 2. - Graphics Basics
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
    dot.StrokeThickness <- 0.0
    Canvas.SetLeft(dot, x - dot.Width / 2.0)
    Canvas.SetTop(dot, y - dot.Width / 2.0)
    canvas.Canvas.Children.Add dot |> ignore

let drawDots canvas fill positions = 
    let grp = GeometryGroup()
    grp.FillRule <- FillRule.Nonzero
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


////////////////////////////////////////
// Part 3. - Drawing Complex Sets

let drawSet canvas fill pred =
    let dotStep = 0.05
    let dots = 
        [for x in -canvas.Size .. dotStep .. canvas.Size do
            for y in -canvas.Size .. dotStep .. canvas.Size do
                if pred {Re = x; Im = y} then yield (x,y) ]
    drawDots canvas fill dots

////////////////////////////
let set1 z =
    z.Re > 0.0  

let set2 z =
    (z.Im < 0.0) && (z.Re >= 0.0) && (abs z < 2.0)

// This is the exercise we did in class
// returns true or false, whether z s in the set or not
let set3 z =
    let four = {Re = 4.0; Im = 0.0}
    let z1 = z - one + (four * i) 
    let r1 = abs z1
    (z.Im < 0.0) && (1.0 < r1) && (r1 < 5.0) 

let main () =
    // Set up the canvas and define some colors
    let canvas = makeCanvas App.Console.Canvas 10.0
    let black = SolidColorBrush(Colors.Black)
    let red = SolidColorBrush(Colors.Red)
    let green = SolidColorBrush(Colors.Green)
    let blue = SolidColorBrush(Colors.Blue)
    let brown = SolidColorBrush(Colors.Brown)
    
    // drawSet canvas red set1
    // drawSet canvas blue set2
    drawSet canvas blue set3
    
    drawAxes canvas 1.0
    App.Console.CanvasPosition <- CanvasPosition.Alone

App.Dispatch (fun() -> main())
