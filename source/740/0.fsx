
type public Class1() =
    [<DefaultValue>]
    val mutable public onCancel : System.Object -> unit
 
    member public this.Verify() = 
        if box(this.onCancel) = null then
            ()//do something
