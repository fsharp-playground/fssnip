open System

type Graph = (int * (int list)) list

let testGraph : Graph =
  [
    (1, [2; 5])
    (2, [1; 5; 3])
    (3, [2; 4])
    (4, [3; 5; 6])
    (5, [1; 2; 4])
    (6, [4])
  ]

let rec bronKerbosch R P X graph =
    let neighbors vertex =
        graph
        |> List.find (fun (v, _) -> v = vertex)
        |> snd
        |> set
    seq {
        if (Set.isEmpty P) && (Set.isEmpty X) then
          yield (Set.toSeq R)
        let vPX =
            Seq.unfold
                (function
                | (v::tailP as P, currX) ->
                    let newX = Set.add v currX
                    Some((v, set <| P, currX), (tailP, newX))
                | ([], _) -> None)
                (P |> Set.toList, X)
        for (v, P, X) in vPX do
            let n = neighbors v
            yield! bronKerbosch (Set.add v R) (Set.intersect P n) (Set.intersect X n) graph
    }

let findMaxCliques graph = bronKerbosch Set.empty (graph |> List.map fst |> set) Set.empty graph

//findMaxCliques testGraph;;
//val it : seq<int> list =
//  [set [1; 2; 5]; set [2; 3]; set [3; 4]; set [4; 5]; set [4; 6]]