open System
open System.IO

let generateCircularSeq (lst:'a list) = 
    let rec next () = 
        seq {
            for element in lst do
                yield element
            yield! next()
        }
    next()

for i in [1;2;3;4;5;6;7;8;9;10] |> generateCircularSeq |> Seq.take 12 do
    i |> Console.WriteLine 