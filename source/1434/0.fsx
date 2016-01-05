let notEmpty s = not (Seq.isEmpty s)

// inclusive version of takeWhile - includes the element which broke the condition
let takeWhileInc cond s =
  seq {
    yield! s |> Seq.takeWhile cond
    let r = s |> Seq.skipWhile cond
    if notEmpty r then yield r |> Seq.head
  }
// inclusive version of skipWhile - also skips the first element which broke the condition  
let skipWhileInc cond s =
  let r = s |> Seq.skipWhile cond
  if notEmpty r then (r |> Seq.skip 1) else r

// split a large sequence into a sequence of sequences, determined by a splitter condition
let rec splitSubSequences cond s =
  seq {
    if not (s |> Seq.isEmpty) then
      yield (s |> takeWhileInc cond)
      yield! (s |> skipWhileInc cond |> splitSubSequences cond)
    }
