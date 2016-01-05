type Oddness =
| IsOdd
| IsEven

let getOddness = 
   function 
   | n when (n % 2)= 0 -> IsEven
   | _ -> IsOdd

getOddness 7 |> printfn "%A"
getOddness 12 |> printfn "%A"