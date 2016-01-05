// A higher order variant of factorize, folding over prime factors of n
// f: state -> factor -> state
let factorf f state n =
    // Yields numbers of the form 6k +/- 1 (and 3)
    let inline next n = 
        match n with
        | 2L -> 3L
        | 3L -> 5L
        | n -> if n % 6L = 5L then n + 2L else n + 4L

    let rec aux state p n =
        if p = n then f state p
        elif p * p > n then f state n
        elif n % p = 0L then aux (f state p) p (n / p)
        else aux state (next p) n

    aux state 2L n

// Factorize a number: fold each prime factor into a list
let factorize = factorf (fun acc p -> p::acc) [] 

// The product of prime factors of n better be equal to n
let sanitycheck n = factorf (*) 1L n = n

// A square-free number contains no repeating prime factors
let squarefree = factorf (fun (flag,prev) p -> (flag && p <> prev, p)) (true,0L) >> fst