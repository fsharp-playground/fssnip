let letters = 
    [
    '0', ['0']
    '1', ['1']
    '2', ['A';'B';'C']
    '3', ['D';'E';'F']
    '4', ['G';'H';'I']
    '5', ['J';'K';'L']
    '6', ['M';'N';'O']
    '7', ['P';'Q';'R';'S']
    '8', ['T';'U';'V']
    '9', ['W';'X';'Y';'Z']
    ]
    |> dict

let rec permuteDigits = function
    | "" -> [""]
    | s ->
        let hd, tl = s.[0], s.Substring(1)
        [for s in permuteDigits(tl) do 
            for c in letters.[hd] -> c.ToString() + s]

permuteDigits "3663"