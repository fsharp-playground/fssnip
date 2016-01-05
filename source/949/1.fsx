module Dominator =
  
  /// A dominator occurs in more than half of an array's length
  let dominator (arr: int []) = 
    // Define an index seq
    let seqIndex  = Seq.init (arr.Length) id
    // Create groups based on key (i.e., the elements of the original arr)
    let seqGroups = Seq.groupBy(fun i -> arr.[i]) seqIndex
    seqGroups 
      // Find all the groups that the keys' occurrence is more than half of the original arr
      |> Seq.filter(fun x -> (snd x) |> Seq.length >= arr.Length/2) 
      // Convert the result as an index array of the dominator
      |> Seq.map snd |> Seq.concat |> Array.ofSeq

  // Testing cases
  let testArray1 = [|7;4;8;2;4;-1;4;4;4;4;6;4|]
  let testArray2 = [|0;2;2;4;0;2;0;0;2;2;2;8;2;2;2;0;0;0|]
  let testArray3 = [|1;2;3;4|]

  (*Although sometimes returning an Option type would be desirable*)
  // [|1; 4; 6; 7; 8; 9; 11|]
  dominator testArray1
  // [|1; 2; 5; 8; 9; 10; 12; 13; 14|]
  dominator testArray2
  // [||]
  dominator testArray3