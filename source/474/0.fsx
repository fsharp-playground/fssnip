let isPrime n =
    match n with
    | _ when n > 3 && (n % 2 = 0 || n % 3 = 0) -> false
    | _ ->
        let maxDiv = int(System.Math.Sqrt(float n)) + 1
        let rec f d i = 
            if d > maxDiv then 
                true
            else
                if n % d = 0 then 
                    false
                else
                    f (d + i) (6 - i)     
        f 5 2