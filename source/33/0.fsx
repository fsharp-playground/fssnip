let lazyValue = lazy ( 2 + 2 )
let actualValue = Lazy.force lazyValue

printfn "%i" actualValue
