module TryParser =
    // convenient, functional TryParse wrappers returning option<'a>
    let tryParseWith tryParseFunc = tryParseFunc >> function
        | true, v    -> Some v
        | false, _   -> None

    let parseDate   = tryParseWith System.DateTime.TryParse
    let parseInt    = tryParseWith System.Int32.TryParse
    let parseSingle = tryParseWith System.Single.TryParse
    let parseDouble = tryParseWith System.Double.TryParse
    // etc.

    // active patterns for try-parsing strings
    let (|Date|_|)   = parseDate
    let (|Int|_|)    = parseInt
    let (|Single|_|) = parseSingle
    let (|Double|_|) = parseDouble

open TryParser

// tests
let parseMe = function
    | Date d   -> printfn "DateTime %A" d
    | Int 42   -> printfn "Bingo!"
    | Int i    -> printfn "Int32 %i" i
    | Single f -> printfn "Single %g" f
    | Double d -> printfn "Double %g" d // never hit, always parsed as float32
    | s        -> printfn "Don't know how to parse %A" s

parseMe "213"
parseMe "2010-02-22"
parseMe "3,0"
parseMe "jslkdfjlkj"
parseMe "42"