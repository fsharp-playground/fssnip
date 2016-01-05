#r @"PresentationCore"
#r @"PresentationFramework"
#r @"WindowsBase"
#r @"System.Xaml"
#r @"UIAutomationTypes"

open System
open System.Windows          
open System.Windows.Controls  
open System.Windows.Media
open System.Windows.Shapes

/// This operator is similar to (|>). 
/// But, it returns argument as a return value.
/// Then you can chain functions which returns unit.
let ($) x f = f x ; x
/// This operator removes parenthesis..
let (=>) (o:IObservable<_>) f = o.Add f

let (<+>) = Observable.merge
/// Similar to applicative functor in Haskell. But the arguments are flipped
let (|+>) ev f = Observable.map f ev

type StackPanel with
  /// Helper function to compose a GUI
  member o.add x = o.Children.Add x |> ignore

/// This container is used by some controls to share a variable.
/// If the value is changed, it fires changed event.
/// Controls should have this instead of their own internal data
type Reactor<'a when 'a : equality>(value:'a) =
  let mutable _value = value
  let changed = Event<'a>()
  member o.Get       = _value
  member o.Set value =
    let old = _value 
    _value <- value
    if old <> _value then _value |> changed.Trigger
  member o.Changed = changed.Publish

/// Volume control , it shows a value and allows you to change it.
type Volume(title:string, range:int * int, re:Reactor<int>) as this =
  inherit StackPanel(Orientation=Orientation.Horizontal)
  do Label(Content=title,Width=50.) |>this.add 
  let label  = Label(Content=re.Get,Width=50.) $ this.add
  let slider = Slider(Minimum=float(fst range), Maximum=float(snd range), TickFrequency=2., Width=127.) $ this.add
  let changedHandler value =
    label.Content <- string value
    slider.Value  <- float value
  do
    slider.ValueChanged => fun arg -> int arg.NewValue |> re.Set
    re.Get     |> changedHandler
    re.Changed => changedHandler

/// Volume control of a color
type ColorVolume (re:Reactor<Color>) as this =
  inherit StackPanel(Orientation=Orientation.Vertical)
  let alpha = Reactor(int re.Get.A)
  let red   = Reactor(int re.Get.R)
  let green = Reactor(int re.Get.G)
  let blue  = Reactor(int re.Get.B)
  do
    alpha.Changed <+> red.Changed <+> green.Changed <+> blue.Changed => fun _ ->
      re.Set(Color.FromArgb(byte alpha.Get,byte red.Get,byte green.Get,byte blue.Get))
    re.Changed => fun color ->
      alpha.Set (int color.A)
      red.Set   (int color.R)
      green.Set (int color.G)
      blue.Set  (int color.B)

    Volume("Alpha", (0,255), alpha) |> this.add
    Volume("Red"  , (0,255), red  ) |> this.add
    Volume("Green", (0,255), green) |> this.add
    Volume("Blue" , (0,255), blue ) |> this.add


[<RequireQualifiedAccess>]
type MyShapes = Rectangle | Ellipse
/// Shape container control which reacts when properties of a shape is changed.
type ShapeContainer(shapes:Reactor<MyShapes>,width:Reactor<int>,height:Reactor<int>,color:Reactor<Color>) as this =
  inherit Label(Width=250., Height=250.)
  let mutable shape = Ellipse() :> Shape
  let setWidth  width  = shape.Width  <- float width
  let setHeight height = shape.Height <- float height
  let setColor  color  = shape.Fill   <- SolidColorBrush(color)
  let initShape () =
    this.Content <- shape
    setWidth  width.Get
    setHeight height.Get
    setColor  color.Get
  let setShape du =
    match du with
      | MyShapes.Rectangle -> shape <- Rectangle()
      | MyShapes.Ellipse   -> shape <- Ellipse  ()
    initShape ()
  do
    initShape ()
    width.Changed  => setWidth
    height.Changed => setHeight
    color.Changed  => setColor 
    shapes.Changed => setShape


/// This StackPanel contains every controls in this program
let stackPanel = StackPanel(Orientation=Orientation.Vertical)

/// Width reactor object
let width = Reactor(120)
Volume("Width",(50, 240),width) |> stackPanel.add // add a volume to the StackPanel

/// Height reactor object
let height = Reactor(80)
Volume("Height",(50, 200),height) |> stackPanel.add // add a volume to the StackPanel

/// Color reactor object
let color = Reactor(Colors.Blue)
ColorVolume(color) |> stackPanel.add // add volumes to the StackPanel

/// Shape reactor object
let shapes = Reactor(MyShapes.Ellipse)
let ellipseButton   = Button(Content="Ellipse")   $ stackPanel.add
let rectangleButton = Button(Content="Rectangle") $ stackPanel.add
ellipseButton.Click   => fun _ -> shapes.Set MyShapes.Ellipse   // add event handler to fire dependency calculation
rectangleButton.Click => fun _ -> shapes.Set MyShapes.Rectangle

// This is a shape control shown in the bottom of this program's window
ShapeContainer(shapes,width,height,color) |> stackPanel.add

// Make a window and show it
let window = Window(Title="F# is fun!",Width=260., Height=420., Content=stackPanel)
window.Show()