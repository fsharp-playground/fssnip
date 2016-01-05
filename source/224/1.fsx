open System.IO

// Wrap File.ReadLines() in a seq so it can be read multiple times
let ls = seq { yield! File.ReadLines(@"c:\windows\system.ini") }

// fs Can't be enumerated more than once or Reset
let fs = File.ReadLines(@"c:\windows\system.ini")

Seq.iter (printfn "%s") ls  // OK
Seq.iter (printfn "%s") ls  // OK
Seq.iter (printfn "%s") fs  // OK
Seq.iter (printfn "%s") fs  // Fail!
