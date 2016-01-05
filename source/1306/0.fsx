let DiffStrings (s1 : string) (s2 : string) =
   let s1', s2' = s1.PadRight(s2.Length), s2.PadRight(s1.Length)

   let d1, d2 = 
      (s1', s2')
      ||> Seq.zip 
      |> Seq.map (fun (c1, c2) -> if c1 = c2 then '-','-' else c1, c2)
      |> Seq.fold (fun (d1, d2) (c1, c2) -> (sprintf "%s%c" d1 c1), (sprintf "%s%c" d2 c2) ) ("","")
   d1, d2

// Example:
// > DiffStrings "The quick brown fox jumps over a lazy dog" "The quick brown fix humps over a lazy digger";;
// val it : string * string =
//   ("-----------------o--j------------------o-   ",
//    "-----------------i--h------------------i-ger")
// > 