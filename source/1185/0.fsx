// It seems that you can specify thread as STA in any place in module top level.
// It would allow debugger to catch TypeInitializationException.
// It would solve some problems around threading when I writing event handlers.
open System
open System.Windows
[<STAThread>] do ()
let wnd = Window()
Application().Run wnd |> ignore