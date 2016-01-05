let products = ["5000171002525","Tuna", 0.59M; "54491014", "Coke", 0.33M]
let stream = Seq.initInfinite (fun _ -> printf ">"; System.Console.ReadLine())
let scans = stream |> Seq.takeWhile (fun s -> s.Length > 0)
let items = 
    scans |> Seq.fold (fun items scan ->
        match products |> List.tryFind (fun (code,_,_) -> scan = code) with
        | Some((_,n,p) as product) -> printfn "%s\t@ %A" n p; product::items
        | None -> printfn "Not found"; items
    ) []
items |> List.sumBy (fun (_,_,price) -> price) |> printf "Total %A"