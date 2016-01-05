open System.Drawing
open System.Windows.Forms

/// Given a float array, display it in a form
let showDigit (data:float[]) = 
  let bmp = new Bitmap(28, 28)
  for i in 0 .. 783 do
    let value = int data.[i]
    let color = Color.FromArgb(value, value, value)
    bmp.SetPixel(i % 28, i / 28, color)
  let frm = new Form(Visible = true, ClientSize = Size(280, 280))
  let img = new PictureBox(Image = bmp, Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.StretchImage)
  frm.Controls.Add(img)