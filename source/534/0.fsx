type MyClass() =
    do printfn "(created !)"
    member self.Print(msg) = printfn "%s" msg

let lazyMyClass = lazy (MyClass())
let instance<'dummy> = lazyMyClass.Value

[<EntryPoint>]
let main _ =
    printfn "F#!F#!"
    instance.Print("First")
    instance.Print("Second")
    0

// Output:
// F#!F#!
// (created !)
// First
// Second