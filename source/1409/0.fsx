// Possible incorrect indentation :-(
let foo = lazy
  printfn "hi"
  1 + 2

// Uh, oh, this looks a bit ugly...
let zoo = 
  lazy
    printfn "hi"
    1 + 2

// Longer, but simpler indentation...
let bar = Lazy.Create(fun _ -> 
  printfn "hi"
  1 + 2 )
