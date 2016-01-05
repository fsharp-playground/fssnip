type A = {name:string; foo:string}
type B = {name:string; bar:string}

let getOneThing name things =
    things
        |> List.filter (fun thing -> thing.name = name)
        |> List.head

let main () =
    [{A.name = "a"; foo = "foo"}]
    |> getOneThing "a"

(*
F# Compiler for F# 3.1 (Open Source Edition)
Freely distributed under the Apache 2.0 Open Source License

/tmp/test.fs(11,8): error FS0001: Type mismatch. Expecting a
    A list -> 'a    
but given a
    B list -> B    
The type 'A' does not match the type 'B'
*)