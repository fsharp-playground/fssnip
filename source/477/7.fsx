open System

/// Given an offset and a radius from that office, 
/// does mChar exist in that part of str?
let inline existsInWin (mChar: char) (str: string) offset rad =
    let startAt = max 0 (offset - rad)
    let endAt = min (offset + rad) (String.length str - 1)  
    if endAt - startAt < 0 then false
    else
        let rec exists index =
            if str.[index] = mChar then true
            elif index = endAt then false
            else exists (index + 1)
        exists startAt

/// The jaro distance between s1 and s2
let jaro s1 s2 =
    
    // The radius is half of the lesser 
    // of the two string lengths rounded up.
    let matchRadius = 
        let minLen = 
            min (String.length s1) (String.length s2) in
              minLen / 2 + minLen % 2

    // An inner function which recursively finds the number  
    // of matched characters within the radius.
    let commonChars (chars1: string) (chars2: string) =
        let rec inner i result = 
            match i with
            | -1 -> result
            | _ -> if existsInWin chars1.[i] chars2 i matchRadius
                   then inner (i - 1) (chars1.[i] :: result)
                   else inner (i - 1) result
        inner (chars1.Length - 1) []

    // The sets of common characters and their lengths as floats 
    let c1 = commonChars s1 s2
    let c2 = commonChars s2 s1
    let c1length = float (List.length c1)
    let c2length = float (List.length c2)
    
    // The number of transpositions within 
    // the sets of common characters.
    let transpositions = 
        let rec inner cl1 cl2 result = 
            match cl1, cl2 with
            | [], _ | _, [] -> result
            | c1h :: c1t, c2h :: c2t -> 
                if c1h <> c2h
                then inner c1t c2t (result + 1.0)
                else inner c1t c2t result
        let mismatches = inner c1 c2 0.0
        // If one common string is longer than the other
        // each additional char counts as half a transposition
        (mismatches + abs (c1length - c2length)) / 2.0

    let s1length = float (String.length s1)
    let s2length = float (String.length s2)
    let tLength = max c1length c2length

    // The jaro distance as given by 
    // 1/3 ( m2/|s1| + m1/|s2| + (mc-t)/mc )
    let result = (c1length / s1length +
                  c2length / s2length + 
                  (tLength - transpositions) / tLength)
                 / 3.0

    // This is for cases where |s1|, |s2| or m are zero 
    if Double.IsNaN result then 0.0 else result

open Xunit

[<Fact>]
let ``Jaro identity test`` () = 
    let result = jaro "RICK" "RICK"
    Assert.Equal("1.000", String.Format("{0:0.000}", result))

[<Fact>]
let ``Jaro martha test`` () =
    let result = jaro "MARTHA" "MARHTA"
    Assert.Equal("0.944", String.Format("{0:0.000}", result))

[<Fact>]
let ``Jaro dwayne test`` () = 
    let result = jaro "DWAYNE" "DUANE"
    Assert.Equal("0.822", String.Format("{0:0.000}", result))

[<Fact>]
let ``Jaro dixon test`` () =
    let result = jaro "DIXON" "DICKSONX"
    Assert.Equal("0.767", String.Format("{0:0.000}", result))

