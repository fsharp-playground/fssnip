module L1 =
  // [snippet:Naive recursive implementation]
  /// Create a list containing results of calling 
  /// the function 'f' on all elements of the input 'list'
  let rec map f list = 
    match list with 
    | x::xs -> (f x)::(map f xs)
    | [] -> []
  // [/snippet]

module L2 =
  // [snippet:Tail-recursive implementation]
  /// Create a list containing results of calling 
  /// the function 'f' on all elements of the input 'list'
  let map f list = 
    /// Inner recursive function that uses
    /// accumulator argument to construct list
    let rec mapAux acc list = 
      match list with 
      | x::xs -> mapAux ((f x)::acc) xs
      | [] -> acc |> List.rev
    mapAux [] list
  // [/snippet]