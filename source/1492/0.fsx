type A = {name:string; foo:string}

type B = {name:string; bar:string}

let getOneThing nameFn name things =
    things
        |> List.filter (fun thing -> (nameFn thing) = name)
        |> List.head

[<EntryPoint>]
let main _ =
    [{B.name = "b"; bar = "bar"}]
    |> getOneThing (fun thing -> thing.name) "b"
    |> printfn "%A"

    [{A.name = "a"; foo = "foo"}]
    |> getOneThing (fun thing -> thing.name) "a"
    |> printfn "%A"

    0