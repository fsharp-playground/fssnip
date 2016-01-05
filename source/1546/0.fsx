let dateRange startDate (endDate:System.DateTime) = 
   Seq.unfold (function 
      | acc when acc < endDate.AddDays(1.) -> Some(acc, acc.AddDays(1.))
      | acc when acc > endDate -> Some(acc, acc.AddDays(-1.))
      | acc -> None) startDate

// Another bit different approach:
// let generate (start:DateTime) x = 
//    [1..x] |> List.map(fun i -> start.AddDays(float i))
