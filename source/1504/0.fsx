open Eto.Forms
open Eto.Drawing


let app = new Application()

let mutable dir = 1
let mutable k = 0

let d = new Drawable()

let t = new UITimer()
t.Interval <- 0.1
t.Elapsed.Add(fun _ -> k <- k+dir; d.Invalidate())

d.Paint.Add(fun e ->
  let g = e.Graphics
  g.ScaleTransform(1.f, -1.f)
  g.TranslateTransform(0.f, -single(d.Size.Height))
  let n = d.Size.Height / 10
  let h = single(n * 10)
  if k = n || k = -1 then dir <- dir * -1
  for c in 0 .. (min k (n - 1)) do
    let x = single(c)*10.f
    g.DrawLine(Colors.Black, 0.f, h - x, x + 10.f, 0.f)
)

let f = new Form(Topmost=true, ClientSize = new Size(600, 480))
f.Content <- d
f.Show()

f.Shown.Add(fun _ -> t.Start())
