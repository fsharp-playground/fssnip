let rnd = System.Random()
let mutable y = 5.6
if rnd.Next(2) = 0 then y <- "Hey big boy"
printfn "%f" y
