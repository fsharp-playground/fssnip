// for a full explanation of this snippet see:
// http://strangelights.com/blog/archive/2011/09/02/calculating-when-the-1000th-xkcd-will-appear.aspx
open System

let epochNumber = 946
let epochDate = new DateTime(2011, 09, 2)
let xckdDays = Set.ofList [ DayOfWeek.Monday; DayOfWeek.Wednesday; DayOfWeek.Friday]

let getXkcdDate n =
    if n < epochNumber then failwithf "n was %i, it must be less that the epoch number %i" n epochNumber
    let n = n - epochNumber
    Seq.initInfinite (fun i -> epochDate.AddDays(float i))
    |> Seq.filter (fun date -> date.DayOfWeek |> xckdDays.Contains)
    |> Seq.nth n

getXkcdDate 1000
//val it : DateTime = 06/01/2012 00:00:00
getXkcdDate 1024
//val it : DateTime = 02/03/2012 00:00:00
getXkcdDate 2000
//val it : DateTime = 28/05/2018 00:00:00

