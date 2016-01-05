let rec dates (fromDate:System.DateTime) (toDate:System.DateTime) = seq {
    if fromDate <= toDate then 
        yield fromDate
        yield! dates (fromDate.AddDays(1.0)) toDate
    }

let doTheThing trader date = ()

let calculateFor days calculation = 
    dates DateTime.Today (DateTime.Today.AddDays(-days))
    |> Seq.map calculation

calculateFor 20. <| doTheThing "Trader"