// Path to Mono's Gtk# library for Xamarin Studio on Mac
//#I "/Library/Frameworks/Mono.framework/Versions/3.10.0/lib/mono/gtk-sharp-2.0"
//#r "gdk-sharp.dll"
//#r "gtk-sharp.dll"
// Path to Mono's Gtk# library for MonoDevelop on Linux
#r "/usr/lib/cli/atk-sharp-2.0/atk-sharp.dll"
#r "/usr/lib/cli/glib-sharp-2.0/glib-sharp.dll"
#r "/usr/lib/cli/gdk-sharp-2.0/gdk-sharp.dll"
#r "/usr/lib/cli/gtk-sharp-2.0/gtk-sharp.dll"

open Gtk
open System

/// Abstract Syntax Tree
module AST =
   type distance = int
   type degrees = int
   type count = int
   type command =
      | Forward of distance
      | Left of degrees
      | Right of degrees
      | Repeat of count * command list

open AST

// Turtle Type
type Turtle = { X:float; Y:float; A:int }

let width, height = 500, 500

let execute commands drawLine =
   let turtle = { X=float width/2.0; Y=float height/2.0; A = -90 }
   let rec perform turtle = function
      | Forward n ->
         let r = float turtle.A * Math.PI / 180.0
         let dx, dy = float n * cos r, float n * sin r
         let x, y =  turtle.X, turtle.Y
         let x',y' = x + dx, y + dy
         drawLine (x,y) (x',y')
         { turtle with X = x'; Y = y' }
      | Left n -> { turtle with A=turtle.A + n }
      | Right n -> { turtle with A=turtle.A - n }
      | Repeat(n,commands) ->
         let rec repeat turtle = function
            | 0 -> turtle
            | n -> repeat (performAll turtle commands) (n-1)
         repeat turtle n
   and performAll = List.fold perform
   performAll turtle commands |> ignore

let show commands =
   let window = new Window("Turtle")
   window.SetDefaultSize(width, height)
   window.DeleteEvent.Add(fun e -> window.Hide(); Application.Quit(); e.RetVal <- true)
   let drawing = new Gtk.DrawingArea()
   drawing.ExposeEvent.Add( fun x ->
       let gc = drawing.Style.BaseGC(StateType.Normal)
       let allocColor (r,g,b) =
          let col = ref (Gdk.Color(r,g,b))
          let _ = gc.Colormap.AllocColor(col, true, true)
          !col
       gc.Foreground <- allocColor (255uy,0uy,0uy)
       let drawLine (x1,y1) (x2,y2) =
            drawing.GdkWindow.DrawLine(gc, int x1, int y1, int x2, int y2) 
       execute commands drawLine
       )
   window.Add(drawing)
   window.ShowAll()
   window.Show()

let invoke action =   
    Application.Init()
    Application.Invoke(fun _ _ -> action())
    Application.Run()

let commands = [for i in 1..1000 do yield! [Forward 6; Right(i*7)]]
invoke (fun () -> show commands)