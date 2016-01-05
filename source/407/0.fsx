open System.Collections.Generic

let fromEnum (input : 'a IEnumerator) = 
    seq {
        while input.MoveNext() do
            yield input.Current
    }

let getMore (input : 'a IEnumerator) = 
    if input.MoveNext() = false then None
    else Some ((input |> fromEnum) |> Seq.append [input.Current])
    
let splitBy (f : 'a -> bool) (input : 'a seq)  = 
    use s = input.GetEnumerator()
    let rec loop (acc : 'a seq seq) = 
        match s |> getMore with 
        | None -> acc
        | Some x ->[x |> Seq.takeWhile (f >> not) |> Seq.toList |> List.toSeq]
                   |> Seq.append acc
                   |> loop
    loop Seq.empty |> Seq.filter (Seq.isEmpty >> not)
    
seq [1;2;3;4;1;5;6;7;1;9;5;5;1]
|> splitBy ( (=) 1) |> printfn "%A"