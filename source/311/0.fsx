open System.Collections.Generic

let seqTake n (s:'a IEnumerator) = 
    let rec loop n = 
        seq {
            if n > 0 && s.MoveNext() then yield s.Current; yield! loop (n-1)
        }
    loop n

let seqWin n (s:'a seq) =
    let e = s.GetEnumerator()
    let rec loop () = 
        seq {
            let lst = e |> seqTake n  |> Seq.toList
            if lst.Length <= n && lst.Length > 0 then 
                yield lst            
                yield! loop() 
        }
    loop ()

[1..5] |> seqWin 2