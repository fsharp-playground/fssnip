open System
open System.Windows.Forms

let lbl = new Label(Text = "Label")
lbl.Height <- 200
let img = 
    let temp = new PictureBox(Top=40, Left=8, Width=128, Height= 128, SizeMode=PictureBoxSizeMode.StretchImage, AllowDrop=true)
    temp.DragEnter.Add(fun args ->
        lbl.Text <- "DragEnter: " + String.Join(", ", args.Data.GetFormats())
        args.Effect <- DragDropEffects.All
        )
    temp.DragDrop.Add(fun args ->
        lbl.Text <- "dropped"
        )
    temp

[<STAThreadAttribute>]
do 
    let frm = new Form()
    let panel = new FlowLayoutPanel()
    panel.Dock <- DockStyle.Fill
    frm.Controls.Add panel
    panel.Controls.Add img
    panel.Controls.Add lbl
    Application.Run frm