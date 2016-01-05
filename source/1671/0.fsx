open FsUnit
open Xunit

[<Fact>]
let ``Compress List``() =

    let compress = function
        | [] -> []
        | ls -> List.fold (fun (acc:list<'a>) e -> if acc.Head = e then acc else e::acc) [ls.Head] ls.Tail |> List.rev

    [] |> compress |> should equal []
    [1; 1; 2; 3; 3; 3; 2; 2; 3] |> compress |> should equal  [1; 2; 3; 2; 3]
    [[1; 2]; [1; 2]; [3; 4]; [1; 2]] |> compress |> should equal  [[1; 2]; [3; 4]; [1; 2]]
    "Leeeeeerrroyyy" |> List.ofSeq |> compress |> should equal "Leroy"