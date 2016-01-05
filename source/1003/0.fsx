// Facet Pattern
// Used for Principle of Least Authority (POLA)
// Inspired by: "The Lazy Programmer's Guide to Secure Computing"
//              http://www.youtube.com/watch?v=eL5o4PFuxTY
type IFacet<'a> =
    abstract Set    : 'a   -> unit
    abstract Get    : unit -> 'a Option
    abstract Revoke : unit -> unit
 
type OnOff = On | Off
 
let allowWrite access f = if access=On then f()
let allowRead  access f = if access=On then Some( f() ) else None
 
/// Revocable Facet Maker
let makeFacet x =
    let access = ref On
    { new IFacet<_> with
        member this.Revoke() =             access := Off            
        member this.Get()    = allowRead  !access (fun () -> !x )
        member this.Set(v)   = allowWrite !access (fun () ->  x := v)
    }
 
// usage
let my = ref "Revocable Facet"
let facet = makeFacet my
 
facet.Get()             = Some "Revocable Facet"
facet.Set( "changed" )
facet.Get()             = Some "changed"
facet.Revoke()       
facet.Get()             = None
facet.Set( "changed2" )
facet.Get()             = None
my.Value                = "changed"