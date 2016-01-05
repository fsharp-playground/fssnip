type A = {name:string; foo:string}

type B = {name:string; bar:string}

let getOneThing name things =
    let inline doStuff (thing:^T when ^T : (member name : string)) = thing.name = name
    things
        |> List.filter (fun thing -> thing.name = name)
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

(*
F# Compiler for F# 3.1 (Open Source Edition)
Freely distributed under the Apache 2.0 Open Source License

/tmp/test.fs(6,70): warning FS0064: This construct causes code to be less
 generic than indicated by the type annotations. The type variable 'T has
 been constrained to be type 'B'.

/tmp/test.fs(18,8): error FS0001: Type mismatch. Expecting a
    A list -> 'a    
but given a
    B list -> B    
The type 'A' does not match the type 'B'
*)