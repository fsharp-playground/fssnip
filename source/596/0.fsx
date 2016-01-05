open System
open System.Collections.Generic

let takeRandomly count lst =
    let random = Random()
    let makeList (xs : 'T seq) = List(xs)
    let list = makeList lst

    let rec takeRandomly count (list : 't List) lst acc =
        match count = acc with
            | true  -> lst
            | false ->
                match list.Count with
                    | 0 -> lst
                    | c ->
                        let c' = c - 1
                        let rand = random.Next(0, c')
                        let item = list.[rand]
                        list.RemoveAt rand
                        let lst' = item :: lst
                        takeRandomly count list lst' (acc + 1)
        
    takeRandomly count list [] 0

// Example:
let list = takeRandomly 50 [0 .. 1000]

// Output:
// val list : int list =
//   [176; 24; 351; 958; 102; 127; 612; 133; 237; 264; 30; 903; 395; 687; 594;
//    459; 80; 707; 391; 210; 953; 144; 685; 536; 329; 437; 659; 129; 948; 758;
//    113; 532; 381; 74; 284; 222; 721; 892; 435; 423; 580; 590; 875; 462; 214;
//    414; 235; 561; 279; 804]