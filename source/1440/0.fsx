[<AutoOpen>]
module ExtraPrimitives =
    let inline tryUnbox<'a> (x:obj) =
        match x with
        | :? 'a as result -> Some (result)
        | _ -> None

let test = box "Hello"

test
|> tryUnbox<string>
|> Option.iter (printfn "%s")