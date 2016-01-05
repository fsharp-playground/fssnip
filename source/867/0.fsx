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
    |> Map.ofList

let numberToWords (digits:string) =
    let rec f acc = function
        | [] -> [acc |> List.rev]
        | x :: xs -> [for c in letters.[x] do yield! f (c::acc) xs]
    f [] [for c in digits -> c]
    |> List.map (fun chars -> System.String(chars |> List.toArray))

let food = numberToWords "3663"