module Seq=
    let breakBy n s = 
        let filter k (i,x) = ((i/n) = k)
        let index = Seq.mapi (fun i x -> (i,x))
        let rec loop s = 
            seq { if not (Seq.isEmpty s) then 
                    let k = (s |> Seq.head |> fst) / n
                    yield (s |> Seq.takeWhile (filter k)
                             |> Seq.map (fun (i,x) -> x))
                    yield! loop (s |> Seq.skipWhile (filter k)) }
        loop (s |> index)

seq {1..25000} |> Seq.breakBy 50
(*
val it : seq<seq<int>> =
  seq
    [seq [1; 2; 3; 4; ...]; seq [51; 52; 53; 54; ...];
     seq [101; 102; 103; 104; ...]; seq [151; 152; 153; 154; ...]; ...]
*)
