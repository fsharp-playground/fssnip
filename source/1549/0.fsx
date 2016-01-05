let line width j =
  List.init
    width
    (fun i -> if abs(width/2 - i) <= j then "*" else " ")
 
let tree n =
  List.init (n/2) (line n) @        // treetop
    List.init 2 (fun _ -> line n 0) // trunk
 
let printTree =
  tree >> Seq.iter (Seq.fold (+) "" >> printfn "%s")
 
printTree 11