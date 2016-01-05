let rec splitFn n groups letters =
  try
    let subSeq = Seq.take n letters
    List.ofSeq subSeq :: splitFn n groups (Seq.skip n letters)
  with
    | _ -> groups

let s = "this is a string"
let splits =
  s.ToCharArray()
  |> List.ofArray

splitFn 3 [] splits
  |> printfn "%A"

(*
$ fsharpi /tmp/test.fsx
[['t'; 'h'; 'i']; ['s'; ' '; 'i']; ['s'; ' '; 'a']; [' '; 's'; 't'];
 ['r'; 'i'; 'n']]
*)