let rec fibs a b = 
    match a + b with 
    | c when c < 10000 -> c :: fibs b c 
    | _ -> [] 

let fibonacci = 0::1::1::2::(fibs 1 2) 
