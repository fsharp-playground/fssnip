let (---) (a:int) (b:string) = 
  (a, b)
let (-->) (a, b) c = 
  sprintf "%d --- %s --> %d" a b c

// Example usage
1 --- "hello" --> 2
