let rec binSearch target arr =
    match Array.length arr with
      | 0 -> None
      | i -> let middle = i / 2
             match  sign <| compare target arr.[middle] with
               | 0  -> Some(target)
               | -1 -> binSearch target arr.[..middle-1]
               | _  -> binSearch target arr.[middle+1..]



binSearch 7918 [|1..10000000|]
