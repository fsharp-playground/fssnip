  let equi lst =
    let rec loop acc p left right = function
      | h::t ->
        let acc = if left = right-h then p::acc else acc
        loop acc (p+1) (left+h) (right-h) t
      | [] -> acc |> List.rev
    loop [] 0 0 (lst |> List.sum) lst

  let listTest = [1;2;3;-1;0;1;-1;5;-5;5;5;15;-5]
  equi listTest
  // Result
  // val it : int list = [10]