let anotherQuickSort lst = 
  let rec QuickSortCont l cont =
    match l with
    | [] -> cont []
    | pivot::rest -> 
      let left, right = rest |> List.partition(fun i -> i < pivot)
      QuickSortCont left (fun accLeft -> 
      QuickSortCont right (fun accRight -> cont(accLeft@pivot::accRight)))
  QuickSortCont lst (fun x -> x)

// Test
anotherQuickSort [-22;2;34;-2;0;9;-5;14;-55;74;13]
// Results
[-55; -22; -5; -2; 0; 2; 9; 13; 14; 34; 74]
