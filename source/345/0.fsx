let (|Default|) defaultValue input =
    defaultArg input defaultValue

//val compile : bool option -> bool
let compile (Default true optimize) = 
    optimize