// Generate text from Philippe Decrauzat's D.T.A.B.T.W.H.A.H.E. 2010
let print (s:string) =
    [|
    for y in 0..s.Length-1 ->
        [|for x in 0..y-1 -> s.[x]
          for x in y..s.Length-1 -> s.[y]
        |]
        |> fun cs -> 
            System.String(cs) + 
            System.String(cs |> Array.rev |> Seq.skip 1 |> Seq.toArray) 
    |] 
    |> fun ys -> [|yield! ys; yield! (Array.rev ys |> Seq.skip 1)|] 
    |> String.concat "\r\n"
print "A HAPPY ENDING" 
|> printfn "%s"