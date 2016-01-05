module thingy =

  let isSumPossible (target :int) (ints : int list)= 
    let head, tail = 
      match ints with 
      | x::xs -> x,xs
      | [] -> 0,List.empty
    let combos = 
      tail 
      |> List.map (fun i -> head,i )
      |> List.filter (fun (i,j) -> (i+j) = target)
    combos 
    |> List.length 
    |> (<>) 0

  let testRandom () =
    let rand = new System.Random()
    let amount = rand.Next(100);
    let sum = rand.Next();	
    let ints = List.init amount (fun i -> rand.Next())
    ints
    |> isSumPossible sum
	
	