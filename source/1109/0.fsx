open System

type IType = 
    inherit IDisposable
    abstract say : string -> unit
 
let St = {
    new IType with
        member i.say hi     = Console.Write hi
        member i.Dispose()  = Console.Write "So I disposed"
    }
 
let Say1(cmon : IType) =
    using   <| cmon
            <| fun lol -> lol.say
 
Say1 St " :( "