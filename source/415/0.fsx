open System.Collections.Generic

//Filter function with accumulator
let filter (acc:'a) (f:('a -> 'b -> bool * 'a)) (s:'b seq) = 
    let rec iter (acc:'a) (e:IEnumerator<'b>) = 
        match e.MoveNext() with
        | false -> Seq.empty 
        | true -> match f acc e.Current with
                  | (true,newAcc) -> seq { yield e.Current; yield! iter newAcc e}
                  | (false,newAcc) -> seq { yield! iter newAcc e}
    iter acc (s.GetEnumerator())

//main function
let skipUntilChange (f : 'a -> 'b) (s : 'a seq) = 
    s |> Seq.skip 1
    |> filter (s |> Seq.head |> f)
        (fun a b -> if a = f b then false,f b else true,f b)

//Example:
[1;1;1;3;3;3;5;5;5] |> skipUntilChange id |> printfn "%A"