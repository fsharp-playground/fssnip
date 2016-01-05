module FibContinuation = 
    let rec fib n count = 
        match n with 
        | 1 
        | 2 -> count (1) 
        | _ ->  
            let first x = 
                let second y = 
                    count(x + y) 
                fib (n-2) second 
            fib (n-1) first 