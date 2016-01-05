/// Get the next in a Conway look-and-say sequence (sequence A005150 in OEIS).
let Next (seed : string) =
    seq {
        let prev, count = ref ((char)0), ref(0)
        let len = seed.Length
        for i in [0..len-1] do
            let c = seed.[i]
            if i = 0 then
                prev := c
            if (c <> !prev) then
                yield (!count), !prev
                count := 0
                prev := c
            if (i = len-1) then
                yield (!count)+1, !prev
            else
                count := !count + 1
    }
    |> Seq.map (fun (count, c) -> sprintf "%i%c" count c)
    |> Seq.fold (fun acc elem -> sprintf "%s%s" acc elem) ""

/// Create a Conway look-and-say sequence (sequence A005150 in OEIS) starting 
///  with a given initial value. (Uses strings rather than ints to avoid quickly 
/// overflowing.)
let ConwaySeq (first : string) =
    Seq.unfold (fun seed -> Some(seed, Next seed)) first

// Examples:

// > ConwaySeq "1" |> Seq.take 5 |> List.ofSeq;;
// val it : string list = ["1"; "11"; "21"; "1211"; "111221"]

// > ConwaySeq "A005150" |> Seq.take 5 |> List.ofSeq;;
// val it : string list =
//   ["A005150"; "1A2015111510"; "111A1210111531151110";
//    "311A1112111031151321153110"; "13211A311231101321151113122115132110"]
