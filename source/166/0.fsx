let byrefArrayElement() = 
  let arr = [| 1; 2; 3; |]; 
  let mutable x = &arr.[0]
  x <- 2 
  printfn "%A" arr 

byrefArrayElement()