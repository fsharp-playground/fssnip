let char_count word =
    word
    |> Seq.groupBy (fun c -> c)
    |> Seq.map (fun (c, occ) -> (c, Seq.length occ))
    |> Map.ofSeq

let anagrams words =
    words
    |> Seq.groupBy (fun word -> char_count word)
    |> Seq.map (fun (char_count, words) -> words)

anagrams ["teams"; "meta"; "mate"; "steam"; "good"; "dog"; "meat"];;