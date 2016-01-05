// Active patterns returning one case can have input arguments
let (|C|_|) arg inp = 
  if arg = 1 then Some inp else None

match 1 with 
| C 1 value -> printfn "inp=1"
| _ -> printfn "other"

// But this is not the case for multi-case patterns
let (|A|B|) arg inp = 
  if arg = 1 then A inp else B inp

match 1 with 
| A 1 value -> printfn "inp=1"
| _ -> printfn "other"
