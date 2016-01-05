//
// Welcome to Try F#. Type your F# script in this window. To run code use:
//
// * Ctrl-Enter or the Run button to send selected text to F# Interactive.
//   If no text is selected the entire script will be sent.
//
// * Ctrl-Shift-Enter to send the current line to F# Interactive.
//
// Part 1. - Complex Basics
// Part 2. - Graphics Basics
// Part 3. - Drawing Complex Sets

// Import Try F# compatibility (enable for Visual Studio)
//#load "Offline.fsx"

////////////////////////////////////////
// Part 1. - Complex Basics

// Define complex type with some operators
type Complex =
    { Re : float;
      Im : float }
    static member (+) (z1, z2) = { Re = z1.Re + z2.Re; Im = z1.Im + z2.Im }
    static member (+) (r1, z2) = { Re = r1 + z2.Re;    Im = z2.Im }
    static member (+) (z1, r2) = { Re = z1.Re + r2;    Im = z1.Im }
    static member (-) (z1, z2) = { Re = z1.Re - z2.Re; Im = z1.Im - z2.Im }
    static member (-) (r1, z2) = { Re = r1 - z2.Re;    Im = - z2.Im }
    static member (-) (z1, r2) = { Re = z1.Re - r2;    Im = z1.Im}
    static member (*) (z1, z2) = { Re = ((z1.Re * z2.Re) - (z1.Im * z2.Im)); Im = ((z1.Re * z2.Im) + (z1.Im * z2.Re)) }
    static member (*) (r1, z2) = { Re = r1 * z2.Re;    Im = r1 * z2.Im }
    static member (*) (z1, r2) = { Re = z1.Re * r2;    Im = z1.Im * r2 }
    static member (/) (z1, z2) = 
        let z2_conj = {Re = z2.Re; Im = -z2.Im}
        let den = (z2 * z2_conj).Re
        let num = z1 * z2_conj
        { Re = num.Re / den; Im = num.Im / den }
    static member (/) ((r1:float), z2) = 
        let z2_conj = {Re = z2.Re; Im = -z2.Im}
        let den = (z2 * z2_conj).Re
        let num = r1 * z2_conj
        { Re = num.Re / den; Im = num.Im / den }
    static member (/) (z1, r2) = {Re = z1.Re / r2; Im = z1.Im / r2}
    static member (~-) z = { Re = -z.Re; Im = -z.Im };;

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
let i = {Re=0.0; Im = 1.0}
let pi = atan2 0.0 -1.0

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
 
let makeGraphic (canvas : Canvas) size =
    let initSize = 250.0
    canvas.HorizontalAlignment <- HorizontalAlignment.Center
    canvas.VerticalAlignment <- VerticalAlignment.Center
    let scale = ScaleTransform()
    scale.ScaleX <- initSize / size
    scale.ScaleY <- -initSize / size
    canvas.RenderTransform <- scale
    let border = Border()
    border.Background <- SolidColorBrush(Colors.Green)
    //border.HorizontalAlignment <- HorizontalAlignment.Center
    //border.VerticalAlignment <- VerticalAlignment.Center
    Canvas.SetLeft(border, -size)
    Canvas.SetTop(border, -size)
    border.Height <- 2.0 * size
    border.Width <- 2.0 * size
    canvas.Children.Add border |> ignore

    let innerCanvas = Canvas()
    innerCanvas.HorizontalAlignment <- HorizontalAlignment.Center
    innerCanvas.VerticalAlignment <- VerticalAlignment.Center
    border.Child <- innerCanvas

    let display = TextBlock()
    display.Text <- "Hello"
    innerCanvas.Children.Add display |> ignore
    let scale = ScaleTransform()
    scale.ScaleX <- 1.0
    scale.ScaleY <- -1.0
    display.RenderTransform <- scale
    display.FontSize <- 10.0 * 2.0 * size / initSize

    Canvas.SetLeft(display, -size)
    Canvas.SetTop(display, size)
    
    let mouseMove (x,y) = 
        display.Text <- sprintf "(%.3f, %.3f)" x y
    border.MouseMove.AddHandler (fun sender mouseArgs -> 
        let p = mouseArgs.GetPosition(innerCanvas)
        mouseMove (p.X, p.Y))
    { Canvas = innerCanvas; Size = size; Scale = 2.0 * size / initSize}

let drawLine graphic brush (x1, y1) (x2, y2) = 
    let line = Shapes.Line()
    line.X1 <- x1
    line.Y1 <- y1
    line.X2 <- x2
    line.Y2 <- y2
    line.Stroke <- brush
    line.StrokeThickness <- 1.0 * graphic.Scale
    graphic.Canvas.Children.Add line |> ignore

let drawDot graphic fill (x, y) = 
    let dot = Shapes.Ellipse()
    dot.HorizontalAlignment <- HorizontalAlignment.Center
    dot.VerticalAlignment <- VerticalAlignment.Center
    dot.Height <- 2.0 * graphic.Scale
    dot.Width <- dot.Height
    dot.Fill <- fill
    dot.StrokeThickness <- 0.0
    Canvas.SetLeft(dot, x - dot.Width / 2.0)
    Canvas.SetTop(dot, y - dot.Width / 2.0)
    graphic.Canvas.Children.Add dot |> ignore

let drawDots graphic fill positions = 
    let grp = GeometryGroup()
    grp.FillRule <- FillRule.Nonzero
    let addDot (x,y) = 
        let dot = EllipseGeometry()
        dot.Center <- Point(x,y)
        dot.RadiusX <- 0.01
        dot.RadiusY <- dot.RadiusX
        grp.Children.Add dot
    List.iter addDot positions
    let shape = Shapes.Path()
    shape.Data <- grp
    shape.Fill <- fill
    shape.StrokeThickness <- 0.0
    shape.HorizontalAlignment <- HorizontalAlignment.Center
    shape.VerticalAlignment <- VerticalAlignment.Center
    //Canvas.SetLeft(dot, x - dot.Width / 2.0)
    //Canvas.SetTop(dot, y - dot.Width / 2.0)
    graphic.Canvas.Children.Add shape |> ignore

let drawText graphic (x, y) text = 
    let tb = TextBlock()
    //tb.HorizontalAlignment <- HorizontalAlignment.Center
    //tb.VerticalAlignment <- VerticalAlignment.Center
    tb.Text <- text
    let scale = ScaleTransform()
    scale.ScaleX <- 1.0
    scale.ScaleY <- -1.0
    tb.RenderTransform <- scale
    tb.FontSize <- 10.0 * graphic.Scale
    Canvas.SetLeft(tb, x)
    Canvas.SetTop(tb, y)
    graphic.Canvas.Children.Add tb |> ignore

let drawAxes graphic tickUnit =
    let darkGray = SolidColorBrush(Colors.DarkGray)
    drawLine graphic darkGray (-graphic.Size, 0.0) (graphic.Size, 0.0)
    drawLine graphic darkGray (0.0, -graphic.Size) (0.0, graphic.Size)
    let ticks = [0.0 .. tickUnit .. graphic.Size] @ [-tickUnit .. -tickUnit .. -graphic.Size]
    let tickSize = tickUnit / 10.0
    List.iter (fun x -> drawLine graphic darkGray (x, -tickSize) (x, tickSize)) ticks
    List.iter (fun y -> drawLine graphic darkGray (-tickSize, y) (tickSize, y)) ticks
    
let drawBitmap graphic colorFunc =
    let getCoordinates =
        let step = graphic.Size * 2.0 / 100.0
        [ for x in -graphic.Size .. step .. graphic.Size do
            for y in -graphic.Size .. step .. graphic.Size do
                yield (x, y) ]
    List.iter (fun p -> drawDot graphic (colorFunc p) p) getCoordinates

let drawTestDot graphic fill =
    drawDot graphic fill (-graphic.Size, graphic.Size)

//
//let drawBitmap graphic colorFunc =
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
//    graphic.Canvas.Children.Add image


////////////////////////////////////////
// Part 3. - Mouse display for Complex Functions

let makeFunctionGraphic (canvas : Canvas) size f =
    let initSize = 220.0
    let scale = 2.0 * size / initSize

    // Set the containing canvas to have the right positioning and scaling and y-axis orientation
    canvas.HorizontalAlignment <- HorizontalAlignment.Center
    canvas.VerticalAlignment <- VerticalAlignment.Center
    let scaleTf = ScaleTransform()
    scaleTf.ScaleX <- initSize / size
    scaleTf.ScaleY <- -initSize / size
    canvas.RenderTransform <- scaleTf

    // Add the stack panel which will contain our innner canvases
    let stackPanel = StackPanel()
    stackPanel.Orientation <- Orientation.Horizontal
    Canvas.SetLeft(stackPanel, -size*2.0)
    Canvas.SetTop(stackPanel, -size)
    canvas.Children.Add stackPanel |> ignore

    // An inner canvas needs a border to receive mouse events
    let addCanvas (stackPanel : StackPanel) background =
        let border = Border()
        border.Background <- background
        border.Height <- 2.0 * size
        border.Width <- 2.0 * size
        border.Margin <- Thickness(0.1, 0.0, 0.1, 0.0)
        let canvas = Canvas()
        canvas.HorizontalAlignment <- HorizontalAlignment.Center
        canvas.VerticalAlignment <- VerticalAlignment.Center
        border.Child <- canvas
        stackPanel.Children.Add border |> ignore
        (canvas, border)

    let leftCanvasBorder = addCanvas stackPanel (SolidColorBrush(Colors.LightGray))
    let rightCanvasBorder = addCanvas stackPanel (SolidColorBrush(Colors.White))

    let leftCanvas = fst leftCanvasBorder
    let rightCanvas = fst rightCanvasBorder

    let addTextBlock (canvas : Canvas) =
        let tb = TextBlock()
        let scaleTransform = ScaleTransform()
        scaleTransform.ScaleX <- 1.0
        scaleTransform.ScaleY <- -1.0
        tb.RenderTransform <- scaleTransform
        tb.FontSize <- 10.0 * scale
        Canvas.SetLeft(tb, -size)
        Canvas.SetTop(tb, size)
        canvas.Children.Add tb |> ignore
        tb

    let leftText = addTextBlock leftCanvas
    let rightText = addTextBlock rightCanvas

    let addDot (canvas:Canvas) fill =
        let dot = Shapes.Ellipse()
        dot.Height <- 5.0 * scale
        dot.Width <- dot.Height
        dot.Fill <- fill
        canvas.Children.Add dot |> ignore
        dot
    
    let leftDot = addDot leftCanvas (SolidColorBrush(Colors.Red))
    let rightDot = addDot rightCanvas (SolidColorBrush(Colors.Green))

    let mouseMove (x,y) = 
        let z = {Re=x; Im=y}
        let fz = f z
        leftText.Text <- sprint z
        rightText.Text <- sprint fz
        Canvas.SetLeft(leftDot, x - leftDot.Width / 2.0)
        Canvas.SetTop(leftDot, y - leftDot.Width / 2.0)
        Canvas.SetLeft(rightDot, fz.Re - leftDot.Width / 2.0)
        Canvas.SetTop(rightDot, fz.Im - leftDot.Width / 2.0)
    
    let leftBorder = snd leftCanvasBorder
    leftBorder.MouseMove.AddHandler 
        (fun sender mouseArgs -> 
            let p = mouseArgs.GetPosition(leftCanvas)
            mouseMove (p.X, p.Y))

    let leftGraphic = { Canvas = leftCanvas; Size = size; Scale = scale}
    let rightGraphic = { Canvas = rightCanvas; Size = size; Scale = scale}

    drawAxes leftGraphic 1.0
    drawAxes rightGraphic 1.0
    ()

let show f () =   
    App.Dispatch (fun() -> 
        App.Console.ClearCanvas ()
        makeFunctionGraphic App.Console.Canvas 10.0 f
        App.Console.CanvasPosition <- CanvasPosition.Alone)

////////////////////////////////////
// Easy to change below here

let f1 z = z
let f2 (z:Complex) = 2.0 * z
let f3 z = i * z
let f4 (z:Complex) = z * z
let f5 (z:Complex) = 1.0 / z
let exp z = 
    {Re = (Math.Exp z.Re) * (Math.Cos z.Im); 
     Im = (Math.Exp z.Re) * (Math.Sin z.Im)}
let sin z =
    1.0 / (2.0 * i) * (exp (i * z) - exp (-i*z))
let cos z =
    1.0 / 2.0  * (exp (i * z) + exp (-i*z))
let sinh z = 
    1.0 / 2.0  * (exp z - exp -z)
let cosh z = 
    1.0 / 2.0  * (exp z + exp -z)

show sin ()
