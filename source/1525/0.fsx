#r "System.Drawing.dll"
#r "System.Windows.Forms.dll"
 
open System
open System.Drawing
open System.Windows.Forms
 
let width, height = 512.0, 512.0

let paperSnowflake () =   
   let image = new Bitmap(int width, int height)
   use graphics = Graphics.FromImage(image)  
   use brush = new SolidBrush(Color.Black)
   graphics.FillRectangle(brush, 0, 0, int width, int height)
   graphics.TranslateTransform(float32 (width/2.0),float32 (height/2.0))   
   let color = Color.FromArgb(128,0,128,255)
   use brush = new SolidBrush(color)
   let rand = Random()
   let polys =
      [for i in 1..12 ->
         let w = rand.Next(20)+1 // width
         let h = rand.Next(20)+1 // height
         let m = rand.Next(h)    // midpoint
         let s = rand.Next(30)   // start
         [|0,s; -w,s+m; 0,s+h; w,s+m|]
      ]
   for i in 0.0..60.0..300.0 do           
      graphics.RotateTransform(float32 60.0)
      let poly points =
         let points = [|for (x,y) in points -> Point(x*5,y*5)|]           
         graphics.FillPolygon(brush,points)
      polys |> List.iter poly 
   image

let show () =
   let image = paperSnowflake ()
   let form = new Form (Text="Paper Snowflake", Width=int width+16, Height=int height+36)  
   let picture = new PictureBox(Dock=DockStyle.Fill, Image=image)
   image.Save(@"C:\temp\Paper.png", Imaging.ImageFormat.Png)
   do  form.Controls.Add(picture)
   form.ShowDialog() |> ignore

show()