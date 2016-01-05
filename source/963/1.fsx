module Suppress

// Returns a sequence which consists of the items from 'values' for which
// no item in 'unwanted' gives true when passed to function (f valueItem unwantedItem).
let suppressBy (f : 'T -> 'U -> bool) (values : seq<'T>) (unwanted : seq<'U>) =
    values
    |> Seq.filter (fun v -> unwanted |> Seq.exists (fun u -> f v u) |> not)

// [0; 1; 2; 5; 6; 7; 8; 9]
let example =
    suppressBy (=) [0..9] [3; 4] |> List.ofSeq

type Prospect = { name : string; phoneNumber: string }
type TPSRecord = { phoneNumber: string }

let prospects = [ {name = "A Smith"; phoneNumber = "01234 56789"}
                  {name = "B Smith"; phoneNumber = "01234 56710"}
                  {name = "C Smith"; phoneNumber = "01234 56711"} ]

let tpsData = [ {phoneNumber = "01234 56710"}
                {phoneNumber = "01234 56712"} ]

// [{name = "A Smith"; phoneNumber = "01234 56789";}
//  {name = "C Smith"; phoneNumber = "01234 56711";}]
let doTpsSuppression =
    prospects |> suppressBy (fun p s -> p.phoneNumber = s.phoneNumber) tpsData
    |> List.ofSeq
        
