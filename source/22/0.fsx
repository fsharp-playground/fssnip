module L1 =
  // [snippet:Naive recursive implementation]
  /// Create a list containing all elements 
  /// of 'list' that match the predicate 'f'
  let rec filter f list = 
    match list with 
    | x::xs when f x -> x::(filter f xs)
    | _::xs -> filter f xs
    | [] -> []
  // [/snippet]

module L2 =
  // [snippet:Tail-recursive implementation]
  /// Create a list containing all elements 
  /// of 'list' that match the predicate 'f'
  let filter f list = 
    /// Inner recursive function that uses
    /// accumulator argument to construct list
    let rec filterAux acc list = 
      match list with 
      | x::xs when f x -> filterAux (x::acc) xs
      | _::xs -> filterAux acc xs
      | [] -> acc |> List.rev
    filterAux [] list
  // [/snippet]