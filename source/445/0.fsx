#time "on"
module Equals =

    let test () =
        let mutable b = false
        let mutable x, y = 0, 2
        for i = 0 to System.Int32.MaxValue / 2 do
            b <- x = y
            //b <- System.Object.Equals(x,  y)
            //b <- x.Equals(y)
        printfn "%A" b

    test ()
