open FsUnit
open Xunit

[<Fact>]
let transform() =
    let inline replace1 list a b = list |> Seq.map (fun x -> if x = a then b else x)
    let inline replace2 list func = list |> Seq.map func

    replace1 "hello, how are you?" 'e' 'a' |> should equal "hallo, how ara you?"
    replace1 [1;2;3;4;5;4;3;2;1] 3 9 |> should equal   [1;2;9;4;5;4;9;2;1]

    replace2 "hello, how are you?" (fun x -> if x = 'e' then 'a' else x) |> should equal "hallo, how ara you?"
    replace2 [1;2;3;4;5;4;3;2;1] (fun x -> if x = 3 then 9 else x) |> should equal  [1;2;9;4;5;4;9;2;1]