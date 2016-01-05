type BadBaseClass =
    val mutable state : string 
    new () as o = 
        { state = "BadBaseClass" }
           then o.SetState()
    abstract SetState : unit -> unit
    default o.SetState() = ()   

type DerivedFromBad =
    inherit BadBaseClass
    new () as o = 
        { inherit BadBaseClass();  } 
            then o.state <- "DerivedFromBad "
            
    override o.SetState() =
            stdout.WriteLine(o.state)

let b = new DerivedFromBad()
