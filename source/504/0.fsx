open System

Seq.unfold (fun x ->
                if x > DateTime (2000,12,31) then
                    None
                else
                    Some (x, x.AddMonths(1)))(DateTime(1901,1,1))
|> Seq.filter (fun x -> x.DayOfWeek = DayOfWeek.Sunday)
|> Seq.length |> printfn "Problem 19 answer: %d"