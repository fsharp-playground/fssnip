#time "on"
module Tupled =
    let foo (f : int * int -> int) = 
        f (1, 2)

    let bar (a : int, b : int) =
        a + b

    let test () =
        let mutable x = 0
        for i in 1 .. 2000000000 do
            x <- x + foo bar
        printfn "%A" x
        
    test ()


#time "on"
module Curried =
    let foo (f : int -> int -> int) = 
        f 1 2

    let bar (a : int) (b : int) =
        a + b

    let test () =
        let mutable x = 0
        for i in 1 .. 2000000000 do
            x <- x + foo bar
        printfn "%A" x
        
    test ()


#time "on"
// Not the same but notice the amount of gen0 GC's
module TupledReturn =
    let foo (f : int -> int -> int * int) = 
        f 1 2

    let bar (a : int) (b : int) =
        a, b

    let test () =
        let mutable x = 1, 2
        for i in 1 .. 2000000000 do
            x <- foo bar
        printfn "%A" x
        
    test ()
