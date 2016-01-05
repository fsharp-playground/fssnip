// Observer Pattern
module StringWithEvent =

    type StringChangedEvent() =
        member val RegisteredFunctions = new ResizeArray<string*string->unit>() with get
        member S.Add f = S.RegisteredFunctions.Add f

    type String( s :string ) =
        let mutable str = s
        member val Changed = new StringChangedEvent() with get
        member S.Value
            with get() = str
            and set v =
                if str <> v then
                    for f in S.Changed.RegisteredFunctions do
                        f (str,v)
                    str <- v

// Testing
let str = StringWithEvent.String("Ok")
str.Changed.Add(fun (oldStr, newStr) ->
                    printfn "String changed from %A to %A" oldStr newStr )
str.Value <- "Okey"