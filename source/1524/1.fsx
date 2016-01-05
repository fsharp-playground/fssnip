#r "System.Drawing.dll"
#r "System.Windows.Forms.dll"
 
open System
open System.Drawing
open System.Windows.Forms
 
let width, height = 512, 512

let snowflake (graphics:Graphics) length =
   use pen = new Pen(Color.White)
   let angle = ref 0.0
   let x = ref ((float width/2.0) - length/2.0)
   let y = ref ((float height/2.0) - length/3.0)   
   let rec segment n depth =
      if depth = 0 then
         line n
      else        
         segment (n/3.0) (depth-1)
         rotate -60.0                  
         segment (n/3.0) (depth-1)
         rotate 120.0         
         segment (n/3.0) (depth-1)
         rotate -60.0         
         segment (n/3.0) (depth-1)
   and line n =
      let r = !angle * Math.PI / 180.0
      let x2 = !x + cos(r) * n
      let y2 = !y + sin(r) * n
      graphics.DrawLine(pen, float32 !x,float32 !y, float32 x2, float32 y2)
      x := x2
      y := y2
   and rotate a =
      angle := !angle + a
   let depth = 5
   segment length depth
   rotate 120.0  
   segment length depth  
   rotate 120.0
   segment length depth

let draw () =   
   let image = new Bitmap(width, height)
   use graphics = Graphics.FromImage(image)  
   use brush = new SolidBrush(Color.Black)
   graphics.FillRectangle(brush, 0, 0, width, height)  
   snowflake graphics 360.0
   image

let show () =
   let image = draw ()
   let form = new Form (Text="Koch Snowflake", Width=width+16, Height=height+36)  
   let picture = new PictureBox(Dock=DockStyle.Fill, Image=image)
   image.Save(@"C:\temp\Koch.png", Imaging.ImageFormat.Png)
   do  form.Controls.Add(picture)
   form.ShowDialog() |> ignore
 
show()