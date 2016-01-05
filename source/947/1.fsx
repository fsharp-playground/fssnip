module MaxSubList = 
  
  /// Divide the input list into halves
  let split lst =
    let mid = (lst |> List.length)/2
    let left  = lst |> Seq.take mid |> Seq.toList
    let right = lst |> Seq.skip mid |> Seq.toList
    left, right
  
  /// Collect states from mid towards the left end
  let collectState lst =
    List.scanBack(fun x s ->
      let l = s|> fst 
      x::l, (x::l |> List.sum)) lst ([],0)
  
  /// Get the max state from the left list
  let maxList lst = 
    lst |> collectState |> List.maxBy snd |> fst

  /// Collect states from mid towards the right end
  let collectState' lst =
    List.scan(fun s t -> 
      let l = s |> fst
      t::l, (t::l |> List.sum)) ([],0) lst

  /// Get the max state from the right list
  let maxList' lst = 
    lst |> collectState' |> List.maxBy snd |> fst |> List.rev

  /// Crossing maximum subList
  let crossMax left right =
    let maxLeft = maxList left
    let maxRight = maxList' right
    maxLeft@maxRight

  /// Recursive maxSubList
  let rec maxSubList = function
    | [] -> []
    | [x] -> [x]
    | lst -> 
      let left, right = split lst
      let leftMax  = maxSubList left
      let rightMax = maxSubList right
      let crossingMax = crossMax left right
      let sum = List.sum
      if sum leftMax >= sum rightMax && sum leftMax >= sum crossingMax then leftMax
      elif sum leftMax <= sum rightMax && sum crossingMax <= sum rightMax then rightMax
      else crossingMax 

  // Testing cases
  let testList1 = [13;-3;-25;20;-3;-16;-23;18;20;-7;12;-5;-22;15;-4;7]
  let testList2 = [-2; 1; -3; 4; -1; 2; 1; -5; 4]

  // [18; 20; -7; 12]
  maxSubList testList1
  // [4; -1; 2; 1]
  maxSubList testList2