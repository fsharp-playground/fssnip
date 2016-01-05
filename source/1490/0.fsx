type A = {name:string; foo:string}
type B = {name:string; bar:string}

let inline getOneThing name (things:'T list when 'T : (member name : string)) =
    things
        |> List.filter (fun thing -> thing.name = name)
        |> List.head

let main () =
    [{B.name = "b"; bar = "bar"}]
    |> getOneThing "b"
    |> ignore

    [{A.name = "a"; foo = "foo"}]
    |> getOneThing "a"
    |> ignore

(*
F# Compiler for F# 3.1 (Open Source Edition)
Freely distributed under the Apache 2.0 Open Source License

/tmp/test.fs(6,38): warning FS0064: This construct causes code to be less
 generic than indicated by the type annotations. The type variable 'T has
 been constrained to be type 'B'.

/tmp/test.fs(15,8): error FS0001: Type mismatch. Expecting a
    A list -> 'a    
but given a
    B list -> B    
The type 'A' does not match the type 'B'
*)