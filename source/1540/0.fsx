let dateRange (startDate : System.DateTime) (endDate : System.DateTime) =
    let start = startDate.Date
    let finish = endDate.Date
    if start = finish then Seq.singleton start
    else
        let shouldContinue =
            if start < finish then fun d -> d <= finish
            else fun d -> d >= finish
        let moveNext =
            if start < finish then fun (d : System.DateTime) -> d.AddDays(1.0)
            else fun (d : System.DateTime) -> d.AddDays(-1.0)
        let rec getNext (nextDate : System.DateTime) =
            seq {
                if shouldContinue nextDate then
                    yield nextDate
                    yield! getNext (nextDate |> moveNext)
            }
        getNext start