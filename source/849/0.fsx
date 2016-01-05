module List = 
  /// Partition elements of a list using the specified predicate.
  /// The result is a tuple containing elements (from the beginning 
  /// of the list that satisfy the predicate) and the rest of the list.
  let partitionWhile f =
    let rec loop acc = function
      | x::xs when f x -> loop (x::acc) xs
      | xs -> List.rev acc, xs
    loop [] 

// Example use: Note that ' next' is not in the first
// part of the list, because it follows elements that do
// not start with a space!
[" foo"; " bar"; "goo"; "zoo"; " next"]
|> List.partitionWhile (fun s -> s.StartsWith(" "))
