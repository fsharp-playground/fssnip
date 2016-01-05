// just to keep things tidy
let notEmpty s = not (Seq.isEmpty s)

// inclusive version of takeWhile - includes the element which
// broke the condition
let takeWhileInc cond s =
  seq {
    yield! s |> Seq.takeWhile cond
    let r = s |> Seq.skipWhile cond
    if notEmpty r then yield r |> Seq.head
  }
  
// inclusive version of skipWhile - also skips the first element
// which broke the condition  
let skipWhileInc cond s =
  let r = s |> Seq.skipWhile cond
  if notEmpty r then (r |> Seq.skip 1) else r

// split a large sequence into a sequence of sequences, determined
// by a splitter condition
let rec splitSubSequences cond s =
  seq {
    if not (s |> Seq.isEmpty) then
      yield (s |> takeWhileInc cond)
      yield! (s |> skipWhileInc cond |> splitSubSequences cond)
    }

// take a sequence in the form a1;b1;a2;b2;a3;b3 and make sequence
// of tuples (a1,b1);(a2,b2)...
let rec tuplise s =
  seq {
    if not (s |> Seq.isEmpty) then
      yield (s |> Seq.head),(s |> Seq.skip 1 |> Seq.head)
      yield! (s |> Seq.skip 2 |> tuplise)
  }

// convert a list of relative coordinate pairs into absolute coordinates
let buildCoordPairs s =
  let rec innerBuildCoordPairs s (pla,plo) =
    seq {
      if notEmpty s then
        let (la,lo) = s |> Seq.head
        yield (pla+la,plo+lo)
        yield! innerBuildCoordPairs (s |> Seq.skip 1) (pla+la,plo+lo)
    }
  innerBuildCoordPairs s (0.0,0.0)

// take a google encoded polyline and decode to a sequence of absolute
// coordinate (lat,long) pairs
let decodePolyline (s:string) =
  s |> Seq.map (fun ch -> int(ch) - 63)
    |> splitSubSequences (fun d -> (d &&& 0x20) = 0x20)
    |> Seq.map (fun r -> r |> Seq.map (fun d -> d &&& 0x1F))
    |> Seq.map (fun r ->
            r |> Seq.fold (fun (sh,tot) it -> (sh+5),(tot ||| (it<<<sh))) (0,0))
    |> Seq.map snd
    |> Seq.map (fun d -> if (d &&& 0x01) = 0x01
                                      then ~~~(d>>>1) else (d>>>1))
    |> Seq.map (fun d -> float(d) / 1e5)
    |> tuplise
    |> buildCoordPairs
