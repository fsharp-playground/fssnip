module Control =
    let inline update< ^a when ^a :(member BeginUpdate: unit -> unit) 
                       and ^a : (member EndUpdate: unit -> unit)> (this: ^a) f =
        (^a : (member BeginUpdate: unit -> unit) (this))
        f()
        (^a : (member EndUpdate: unit -> unit) (this))

module Example =
    open System.Windows.Forms
    let tv = new TreeView()
    Control.update tv <| fun () ->
            for i in 1..20 do
                tv.Nodes.Add(new TreeNode(i |> string)) |> ignore