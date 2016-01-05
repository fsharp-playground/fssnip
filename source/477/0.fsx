open System

/// Given an offset and a radius from that office, does mChar exist in that part of str?
let inline existsInWindow (mChar: char) (str: string) offset radius =
    let startAt = max 0 (offset - radius)
    let endAt = min (offset + radius) (String.length str - 1)  
    if endAt - startAt + 1 <= 0 then false
    else
        let rec exists index =
            if str.[index] = mChar then true
            elif index = endAt then false
            else exists (index + 1)
        exists startAt

/// The jaro distance between s1 and s2
let jaro s1 s2 =
    
    // The radius is the half lesser of the two string lengths rounded up.
    let matchRadius = 
        let minLen = min (String.length s1) (String.length s2) in
            minLen / 2 + minLen % 2

    // An inner function which recursively finds the number of matched characters 
    // within the radius.
    let commonChars (chars1: string) (chars2: string) =
        let rec inner i result = 
            match i with
            | -1 -> result
            | _ -> if existsInWindow chars1.[i] chars2 i matchRadius
                   then inner (i - 1) (chars1.[i] :: result)
                   else inner (i - 1) result
        inner (chars1.Length - 1) []

    // The sets of common characters and their lengths as floats 
    let common1 = commonChars s1 s2
    let common2 = commonChars s2 s1
    let c1length = float (List.length common1)
    let c2length = float (List.length common2)
    
    // The number of transpositions within the sets of common characters
    let transpositions = 
        let rec inner i result = 
            match i with
            | -1 -> result
            | _ -> if common1.[i] <> common2.[i] then inner (i - 1) (result + 1.0)
                   else inner (i - 1) result
        let mismatches = inner ((min common1.Length common2.Length) - 1) 0.0
        (mismatches + abs (c1length - c2length)) / 2.0

    let s1length = float (String.length s1)
    let s2length = float (String.length s2)

    // The jaro distance as given by 1/3 ( m/|s1| + m/|s2| + (m-t)/m )
    let result = ((c1length / s1length) +
                  (c2length / s2length) + 
                  ((c1length - transpositions) / c1length)) 
                 / 3.0

    // This is for cases where |s1|, |s2| or m are zero 
    if Double.IsNaN result then 0.0 else result