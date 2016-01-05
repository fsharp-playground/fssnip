open System
open System.Windows.Forms


let combine (first:IEvent<'Del,'T>) (second:IEvent<'Del,'T>) (lst:'a list) = 
    let event = new Event<'a>()
    let rec install i = 
        let rec f:IDisposable = first.Subscribe (fun _ -> 
                                                        if i < lst.Length-1 then
                                                            event.Trigger(lst.[i+1])
                                                            f.Dispose() 
                                                            s.Dispose()
                                                            install (i+1)
                                                        )
        and s:IDisposable = second.Subscribe (fun _ -> 
                                                        if i > 0 then
                                                            event.Trigger(lst.[i-1])
                                                            f.Dispose()
                                                            s.Dispose()
                                                            install (i-1)
                                                        )
        ()
    install -1
    event.Publish 

let frm = new Form(Text="Hello world")
let btn = new Button(Text="Button1")
let btn1 = new Button(Text="Button2")
btn |> frm.Controls.Add
btn1 |> frm.Controls.Add
btn1.Top <- 100

[1;2;3;4;5] |> combine btn.Click btn1.Click 
|> Event.add (fun num -> MessageBox.Show (num.ToString()) |> ignore)

frm.ShowDialog() |> ignore