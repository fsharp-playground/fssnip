/// Implements a tail-recursive looping. The argument is a function
/// that returns either Choice1Of2 with the final result or 
/// Choice2Of2 with new set of arguments.
let rec tailrec args f = 
  match f args with
  | Choice1Of2 res -> res
  | Choice2Of2 newArgs -> tailrec newArgs f

/// Tail-recursive function to sum the list written using 'tailrec'
/// (note - this function is *not* marked as recursive itself)
let sumList list =
  tailrec (list, 0) (fun (list, acc) ->
    match list with 
    | [] -> Choice1Of2 acc
    | x::xs -> Choice2Of2 (xs, x + acc))