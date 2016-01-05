module Option
    
    open FSharpx.Option

    /// Accepts an option of option and concatenates them.
    let inline concat x = 
        x >>= id