let margins (lines : seq<string>) =
    let marginWidth (line : string) =
        line.Length - line.TrimStart([|' '|]).Length 
    lines
    |> Seq.map (fun line -> marginWidth line)
    |> Seq.countBy (fun width -> width)
    |> Seq.filter (fun margCount -> fst(margCount) > 0)
    |> Seq.sortBy (fun margCount -> -snd(margCount))

// Example:
let lines = [
                "This has no margin."
                " This has a one-space margin."
                " So does this."
                "      These three lines have a six-space margin."
                "      Again with the six-space margin."
                "      And again."
                " Back to one space."
                "      And finally, another six space."
            ]

// Output: seq [(6, 4); (1, 3)]
lines |> margins