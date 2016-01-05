open System
open System.Drawing
open System.Windows.Forms

module Layout = 
// [snippet:Layout combinators]
  /// Add single control to the layout
  let (!!) (ctrl:#Control) (x, y) =
    ctrl.Left <- x
    ctrl.Top <- y
    x + ctrl.Width, y + ctrl.Height, [ctrl :> Control]

  /// Add border around the specified layout
  let margin (sizew, sizeh) f (x, y) = 
    let w, h, ctrls = f (x + sizew, y + sizeh)
    w + sizew, h + sizeh, ctrls

  /// Add controls in the layout to a control or form
  let createLayout layout = 
    layout (0, 0) |> ignore

  /// Place two layouts beside each other horizontally
  let ( <||> ) f1 f2 (x, y) = 
    let w1, h1, ctrls1 = f1 (x, y)
    let w2, h2, ctrls2 = f2 (w1, y)
    max w1 w2, max h1 h2, ctrls1 @ ctrls2

  (*[omit:(Other composition combinators omitted)]*)
  /// Place two layouts beside each other vertically
  let ( <=> ) f1 f2 (x, y) = 
    let w1, h1, ctrls1 = f1 (x, y)
    let w2, h2, ctrls2 = f2 (x, h1)
    max w1 w2, max h1 h2, ctrls1 @ ctrls2

  /// Place two layouts over each other
  let ( <//> ) f1 f2 (x, y) =  
    let w, h, ctrls1 = f1 (x, y)
    let _, _, ctrls2 = f2 (0, 0)
    match ctrls1 with
    | [single:Control] -> single.Controls.AddRange(Array.ofSeq ctrls2)
    | _ -> failwith "Children can be added on a single control layout only" 
    w, h, ctrls1(*[/omit]*)

  /// Creates a rectangle control filled with the specified color
  let rectangle (w, h) clr = 
    !! (new Control(BackColor = clr, Width = w, Height = h))
          
  /// Create label using the specified font         
  let label (w, h) fnt s = 
    !!(new Label(Text = s, Font = fnt, Width = w, Height = h, 
                 TextAlign = ContentAlignment.MiddleLeft))
  
  /// Normal system font
  let normal = SystemFonts.DefaultFont
  /// Bold version of normal font
  let title = new Font(SystemFonts.DialogFont, FontStyle.Bold)
  /// Larger and bold version of normal font
  let head = new Font(SystemFonts.DialogFont.FontFamily, 10.0f, FontStyle.Bold)
// [/snippet]
open Layout

do
  // [snippet:Sample dialog window]
  let form = new Form(TopMost = true, Visible = true, Width = 500)
  let name = new TextBox()
  let msg = new TextBox()
  !! form <//> 
  ( ( rectangle (500, 40) Color.White <//>
      margin (10, 10) ( label (500, 20) head "Hello world!") ) <=>
    margin (10, 15)
      ( label (450, 36) normal 
          ("This sample demonstrates how to create a simple layout using combinators" +
           "in F#. Please enter some information to these two boxes:") <=>
        margin (10, 5)
          ( ( label (100, 22) title "Name: " <||> !!name) <=> 
            ( label (100, 22) title "Message: " <||> !!msg) ) ) )
  |> createLayout
  // [/snippet]