
let sa = [ 1 .. 10 ]

let iter2 fs xs =
    let apply1thenn fs  = 
        let last = ref Unchecked.defaultof<_>
        let unforget fs = 
            seq { for x in fs do last := x; yield x }
        let rec forever f = seq { yield f; yield! forever f}

        seq { yield! unforget fs
              yield! forever !last} 

    let fs = (apply1thenn fs)
    Seq.zip fs xs |> Seq.iter(fun (f,e) -> f e)

sa |> iter2 [(fun x -> printfn "%A" x); (fun x -> printfn "another %A" x)]


