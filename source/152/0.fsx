open System

// [snippet:Disposable builder declaration]
type DisposableBuilder() =
  member x.Delay(f : unit -> IDisposable) = 
    { new IDisposable with 
        member x.Dispose() = f().Dispose() }
  member x.Bind(d1:IDisposable, f:unit -> IDisposable) = 
    let d2 = f()
    { new IDisposable with 
        member x.Dispose() = d1.Dispose(); d2.Dispose() }
  member x.Return(()) = x.Zero()
  member x.Zero() =
    { new IDisposable with 
        member x.Dispose() = () }

let disposable = DisposableBuilder()
// [/snippet]

// [snippet:Demo #1: Resetting console colors]
// Creates disposable that resets console color when disposed
let resetColor() = 
  let clr = Console.ForegroundColor
  disposable { Console.ForegroundColor <- clr }

// Prints 'doing work' in red and resets color back
let demo1() =
  use unwind = resetColor()
  Console.ForegroundColor <- ConsoleColor.Red
  printfn "doing work"

demo1()        // Prints 'doing work' in red
printfn "done" // Prints 'done' in original color
// [/snippet]

// [snippet:Demo #2: Composing disposables]
// Create two IDisposable objects that do some cleanup
let cleanup1 = disposable { 
  printfn "cleanup #1" }
let cleanup2 = disposable { 
  printfn "cleanup #1" }

let demo2() =
  // Dispose of both 'cleanup1' and 'cleanup2' when the
  // method finishes. This is useful for example when working
  // with IObservable (to dispose of event registrations)
  use d = disposable { 
    printfn "cleanup"
    do! cleanup1
    do! cleanup2 }
  printfn "foo"

demo2()
// [/snippet]