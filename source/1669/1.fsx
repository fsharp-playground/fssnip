open FsUnit
open Xunit

[<Fact>]
let Transform() =
    let inline replace1 list a b = list |> Seq.map (fun x -> if x = a then b else x)
    let inline replace2 list func = list |> Seq.map func

    let inline m3 tr el time = 
        let mv = tr el 
        if mv = el then [el] else [for x in 1..time -> mv]
    let inline replace3 list tr time =  
        let db = List.foldBack(fun el acc -> (m3 tr el time)::acc) list [];
        [for x in db do yield! x]

    replace1 "hello, how are you?" 'e' 'a' |> should equal "hallo, how ara you?"
    replace1 [1;2;3;4;5;4;3;2;1] 3 9 |> should equal [1;2;9;4;5;4;9;2;1]

    replace2 "hello, how are you?" (fun x -> if x = 'e' then 'a' else x) |> should equal "hallo, how ara you?"
    replace2 [1;2;3;4;5;4;3;2;1] (fun x -> if x = 3 then 9 else x) |> should equal  [1;2;9;4;5;4;9;2;1]
    
    replace3 (List.ofSeq "hello, how are you?") (fun x -> if x = 'e' then 'a' else x)  3 |> should equal "haaallo, how araaa you?"
    replace3 [1;2;3;4;5;4;3;2;1] (fun x -> if x = 3 then 9 else x) 3 |> should equal [1;2;9;9;9;4;5;4;9;9;9;2;1] 