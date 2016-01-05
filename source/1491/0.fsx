type IThing =
    abstract member name : string

type A = 
    {name:string; foo:string}
    interface IThing with
        member this.name = this.name

type B =
    {name:string; bar:string}
    interface IThing with
        member this.name = this.name

let getOneThing name things =
    things
        |> List.filter (fun thing -> (thing :> IThing).name = name)
        |> List.head

[<EntryPoint>]
let main _ =
    [{B.name = "b"; bar = "bar"}]
    |> getOneThing "b"
    |> printfn "%A"

    [{A.name = "a"; foo = "foo"}]
    |> getOneThing "a"
    |> printfn "%A"

    0