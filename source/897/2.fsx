  module FCA =
 
    // Define the Attribute type
    type Attribute =
      | Composite
      | Even
      | Odd
      | Prime
      | Square
 
    // Filter out a composite object set
    let composite objSet =
      let factor n =
        let rec find i =
          if i>=n then false
          elif (n % i = 0) then true
          else find (i + 1)
        find 2
      objSet |> Set.filter(fun i -> factor i)
 
    // Filter out an even object set
    let even objSet =
        objSet |> Set.filter(fun i -> i%2=0)
 
    // Filter out an odd object set
    let odd objSet =
        objSet |> Set.filter(fun i -> i%2<>0)
 
    // Filter out a prime object set
    let prime objSet =
        objSet |> composite |> Set.difference objSet |> Set.remove 1
 
    // Filter out a square object set
    let square objSet =
      let findSqure n =
        let rec find i =
          if i > n then false
            elif (n = i * i) then true
            else find (i + 1)
        find 1
      objSet |> Set.filter(fun i -> findSqure i)
 
    // Build a set of concept pairs
    let conceptPairs attSet objSet =
      attSet |> Set.map(fun attr ->
        match attr with
        | Composite as c -> (c, composite objSet)
        | Even as e      -> (e, even objSet)
        | Odd  as o      -> (o, odd objSet)
        | Prime  as p    -> (p, prime objSet)
        | Square as s    -> (s, square objSet)
        )
 
    // Helper
    let transAttr attSet =
      attSet |> Set.map(fun attr ->
         match attr with
         | Composite -> "C"
         | Even      -> "E"
         | Odd       -> "O"
         | Prime     -> "P"
         | Square    -> "S"
        )
 
    // Create a unmarked working table
    let allocate (objSet: Set<int>) (attSet: Set<Attribute>) =
      let row = objSet.Count
      let col = attSet.Count
      let tableLatt = Array2D.create (row+1) (col+1) " "
      tableLatt
      |> Array2D.iter(fun _ ->
           objSet |> Set.toArray |> Array.iteri(fun i item->
             tableLatt.[i+1, 0] <- string item)
           attSet |> transAttr |> Set.toArray |> Array.iteri(fun i item ->
               tableLatt.[0, i+1] <- item
             )     
           )
      tableLatt
 
    // Construct the lattice table
    let lattTable (objSet: Set<int>) (attSet: Set<Attribute>) =
      let row = objSet.Count
      let workingTable = allocate objSet attSet
      let conceptPairsSet = conceptPairs attSet objSet
      workingTable
      |> Array2D.iteri(fun i _ _ ->
        let objArr = 
          conceptPairsSet |> Set.toArray |> Array.map(fun (x, y) -> y) |> Array.map Set.toArray
        objArr |> Array.iteri(fun ind arr ->
           arr |> Array.iteri(fun indx _  ->
             for i in 1 .. row do
             if arr.[indx] = i then
               workingTable.[i,ind+1] <- "X"
             else
               ()
      )))
      workingTable
    
    let showLatticeTable objSet attrSet = printfn "%A" (lattTable objSet attrSet)

    // Create a set of attributes
    let attributeSet = set [Composite; Even; Odd; Prime; Square]

    // Create a set of objects
    let objSet = set [1 .. 10]

    // Show the lattice table as the result, "X" indicates there is a concept
    // e.g., 9 is Composite, Odd and Square number
    showLatticeTable objSet attributeSet

(*
[[" "; "C"; "E"; "O"; "P"; "S"]
 ["1"; " "; " "; "X"; " "; "X"]
 ["2"; " "; "X"; " "; "X"; " "]
 ["3"; " "; " "; "X"; "X"; " "]
 ["4"; "X"; "X"; " "; " "; "X"]
 ["5"; " "; " "; "X"; "X"; " "]
 ["6"; "X"; "X"; " "; " "; " "]
 ["7"; " "; " "; "X"; "X"; " "]
 ["8"; "X"; "X"; " "; " "; " "]
 ["9"; "X"; " "; "X"; " "; "X"]
 ["10"; "X"; "X"; " "; " "; " "]]
val it : unit = ()
*)