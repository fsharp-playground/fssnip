open System.Collections.Generic 

let filter (acc:'a) (f:('a -> 'b -> bool * 'a)) (s:'b seq) = 
    let rec iter (acc:'a) (e:IEnumerator<'b>) = 
        match e.MoveNext() with
        | false -> Seq.empty 
        | true -> match f acc e.Current with
                  | (true,newAcc) -> seq { yield e.Current; yield! iter newAcc e}
                  | (false,newAcc) -> seq { yield! iter newAcc e}
    iter acc (s.GetEnumerator())

//Example usage
[1;2;3;4;5] |> filter 0 (fun a b -> let newA = a + 1
                                    if newA > 3 then (true,newA)
                                    else (false,newA))
|> Seq.iter (fun i -> printfn "%d" i)