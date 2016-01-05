module Seq =
  let groupAdjacent f (input:seq<_>) = seq {
    use en = input.GetEnumerator()
    let running = ref true
    let rec group() = 
      [ let prev = en.Current
        if en.MoveNext() then
          if (f (prev,en.Current)) then  yield prev; yield! group() else yield prev
        else
            yield prev 
            running := false ]
    if en.MoveNext() then
      while running.Value do
        yield group() |> Seq.ofList }
(*
["a"; "a"; "a"; "b"; "c"; "c"] |> Seq.groupAdjacent (fun (a,b)->a=b)
val it : seq<seq<string>> = seq [["a"; "a"; "a"]; ["b"]; ["c"; "c"]]
*)
