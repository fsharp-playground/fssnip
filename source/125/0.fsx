// Active pattern that can be used to assign 
// values to symbols in a pattern
let (|Let|) value input = (value, input)

// This is useful when writing complex pattern matching
let flag, num = (*[omit:(...)]*)true, 0(*[/omit]*)
match flag, num with
| true, (Let "one" (str, 1) | Let "one" (str, 2) | Let "one" (str, 3)) ->
    // Called when number is between 1 and 3 and assigns textual 
    // representation of the number to 'str' (so that we can handle all
    // cases with just a single match clause)
    printfn "%s" str
| _ -> 
    printfn "Something else"