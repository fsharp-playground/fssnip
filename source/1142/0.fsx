// Simple and quite fast on many data sets
// Slow on small numbers
let clz n =
    let rec loop m c = match m with
                       | 0  -> 32
                       | m when m < 0 -> c
                       | _  -> loop (m <<< 1) (c + 1)
    loop n 0


// Faster method with less speed variation
let clzImmutable n =
    let rec loop m c = match m with
                       | 0  -> 32
                       | m when m < 0 -> c
                       | m when (m &&& 0xFFFF0000) = 0 -> loop (m <<< 16) (c + 16)
                       | m when (m &&& 0xFF000000) = 0 -> loop (m <<< 8) (c + 8)
                       | m when (m &&& 0xF0000000) = 0 -> loop (m <<< 4) (c + 4)
                       | m when (m &&& 0xC0000000) = 0 -> loop (m <<< 2) (c + 2)
                       | _  -> loop (m <<< 1) (c + 1)
    loop n 0


// Perfect hashing using a de Bruijn sequence
// About the same speed as clzImmutable
// Very slow when the look-up table is inside the function
// Constant time, except on 0
let bruijn =[|31; 30; 3; 29; 2; 17; 7; 28; 1; 9; 11; 16; 6; 14; 27; 23; 
              0; 4; 18; 8; 10; 12; 15; 24; 5; 19; 13; 25; 20; 26; 21; 22|]

let clzDeBruijn n = match n with 
                    | 0 -> 32
                    | _ ->  let v = uint32 n
                            let v = v ||| (v >>> 1)
                            let v = v ||| (v >>> 2)
                            let v = v ||| (v >>> 4)
                            let v = v ||| (v >>> 8)
                            let v = v ||| (v >>> 16)
                            let v = (v >>> 1) + 1u
                            bruijn.[int32 ((v * 0x077CB531u) >>> 27)]

    
// Generally the fastest, except on 0
let clzMutable n =
    let mutable c = 0
    let mutable m = n
    if (m &&& 0xFFFF0000) = 0 then  m <- m <<< 16; c <- c + 16
    if (m &&& 0xFF000000) = 0 then  m <- m <<<  8; c <- c +  8
    if (m &&& 0xF0000000) = 0 then  m <- m <<<  4; c <- c +  4
    if (m &&& 0xC0000000) = 0 then  m <- m <<<  2; c <- c +  2
    if (m &&& 0x80000000) = 0 then  m <- m <<<  1; c <- c +  1
    if m = 0 then c <- c + 1
    c
