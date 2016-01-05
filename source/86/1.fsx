let tuple2 x y = x, y
module Seq=
    let trim n = Seq.map snd << Seq.filter (fst >> (<=) n) << Seq.mapi tuple2
    let breakBy n s = 
        let rec loop s = 
            seq { if not (Seq.isEmpty s) then 
                    yield (s |> Seq.truncate n)
                    yield! loop (s |> trim n) }
        loop s

seq {1..25000} |> Seq.breakBy 50
(*
val it : seq<seq<int>> =
  seq
    [seq [1; 2; 3; 4; ...]; seq [51; 52; 53; 54; ...];
     seq [101; 102; 103; 104; ...]; seq [151; 152; 153; 154; ...]; ...]
*)
