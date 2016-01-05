let rec splitFn n groups letters =
  let subSeq = Seq.truncate n letters
  match (Seq.length subSeq) with
    | x when x = n -> List.ofSeq subSeq :: splitFn n groups (Seq.skip n letters)
    | _ -> groups
