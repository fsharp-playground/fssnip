C# code:
public class Class1
    {
        public delegate void CancelHandler(Object sender);
        public CancelHandler onCancel;
 
        public void Verify()
        {
            if (onCancel == null)
            {
                //do something;
            } 
        } 
    }
		
Usually , the null is converted to option type in F# , but in this situation , that’s a little  difficult to make it works.
	
F# code:

type public Class1() =
    [<DefaultValue>]
    val mutable public onCancel : System.Object -> unit
 
    member public this.Verify() = 
        if box(this.onCancel) = null then
            ()//do something
