//using bit pattern to generate subsets
let max_bits x = 
    let rec loop acc = if (1 <<< acc ) > x then acc else loop (acc + 1)
    loop 0
        
let bit_setAt i x = ((1 <<< i) &&& x) <> 0
let subsets s = 
        let a = Set.toArray s in
        let len = (Array.length a)
        let as_set x =  set [for i in 0 .. (max_bits x) do 
                                if (bit_setAt i x) && (i < len) then  yield a.[i]]
        
        seq{for i in 0 .. (1 <<< len)-1 -> as_set i}

// Seq.iter (printf "%O") (subsets (set [1 .. 5])) ;;