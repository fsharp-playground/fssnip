module List

/// Partition a list into two groups. The first contains elements 
/// until an element matching a specified predicate is found and the
/// second group contains all remaining elements.
let partitionUntil p input = 
  let rec loop acc = function
    | hd::tl when p hd -> List.rev acc, hd::tl
    | hd::tl -> loop (hd::acc) tl
    | [] -> List.rev acc, []
  loop [] input


/// A function that nests items of the input sequence 
/// that do not match a specified predicate 'f' under the 
/// last item that matches the predicate. 
let nestUnderLastMatching f input = 
  let rec loop input = seq {
    let normal, other = partitionUntil f input
    match List.rev normal with
    | last::prev ->
        for p in List.rev prev do yield p, []
        let other, rest = partitionUntil (f >> not) other
        yield last, other 
        yield! loop rest
    | [] when other = [] -> ()
    | _ -> invalidArg "" "Should start with true" }
  loop input |> List.ofSeq