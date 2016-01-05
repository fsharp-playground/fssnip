open System
open System.Windows.Forms

let frm = new Form()

let selectMany (f:'T -> IEvent<'Del,'T>) (src:IEvent<'Del,'T>) = 
    let customEvent = new Event<'Del,'T>()
    src
    |> Event.add (fun e -> f(e) |> Event.add (fun t -> customEvent.Trigger(null,t)))
    customEvent.Publish 

let takeUntil (takeTill:IEvent<'Del,'T>) (src:IEvent<'Del,'T>) = 
    let customEvent = new Event<'Del,'T>()
    let s = src.Subscribe (fun e -> customEvent.Trigger(null,e))
    let z = takeTill.Subscribe(fun e -> s.Dispose())
    customEvent.Publish

frm.MouseDown
|> selectMany (fun e -> takeUntil frm.MouseUp frm.MouseMove)
|> Event.add (fun e -> frm.Text <- e.X.ToString() + " -- " + e.Y.ToString())

frm.ShowDialog() |> ignore
