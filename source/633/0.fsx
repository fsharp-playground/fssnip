let fib n =
    match n with
    | 1 -> 1
    | 2 -> 1
    | _ when n <= 1 -> 0
    | _ ->
        let rec iter a b i =
            if i=n then a+b
            else iter b (a+b) (i+1)
        in  
        iter 1 1 3
