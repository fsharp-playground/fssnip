// opening required namespaces
open System
open System.Windows.Forms
open System.Drawing

// creating form
let f = new Form(Visible = true, Text = "TextForm",
                    TopMost = true, Size = Size(800,600))
let tb = new RichTextBox(Dock = DockStyle.Fill, Text = "",
                         Font = new Font(family = new FontFamily("Consolas"), emSize = 16.0f))
f.Controls.Add tb
f.Show()

// define show
let show text =
    tb.Text <- sprintf "%A" text

// examples
show("new text")
// or use pipeline-operator
"newest text" |> show
(1,2,3) |> show
let x = [ yield "one"
          if DateTime.Now.DayOfWeek = DayOfWeek.Monday then
            yield "two"
          yield "three" ]
x |> show