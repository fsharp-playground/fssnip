open System
open System.IO
open System.Threading

let print (message : obj) = Console.WriteLine message

let usable (work : Lazy<unit>) : IDisposable =
    { new IDisposable with
        member this.Dispose () : unit =
            work.Value
    }

let main () =
    use __ = usable (lazy print "Disposing!")
    print "begin work"
    Thread.Sleep 1500
    //failwith "boom!"
    print "end work"

main ()
