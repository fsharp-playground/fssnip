open System.Text.RegularExpressions

let matchPosition str pattern =
    let m = Regex(pattern).Match(str)
    match m.Success with
    | true ->
        let index = m.Index
        let lines = str.Split '\n'
        let rec getLineCol idx len =
            let l = lines.[idx].Length + 1
            let len' = l + len
            if len' > index then
                idx + 1, l - (len' - index) + 1
            else
                getLineCol (idx + 1) len'   
        
        let lineCol = getLineCol 0 0
        Some lineCol
    | false -> None

// Example
let text = "This is a test."
let position = matchPosition text "a"
match position with
| Some (line, col) -> printfn "Match found in line %d, column %d." line col
| None -> ()

// Output:
// Match found in line 1, column 9.
// val text : string = "This is a test."
// val position : (int * int) option = Some (1, 9)