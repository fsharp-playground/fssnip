open System

type Part = Days of int
          | Hours of int
          | Minutes of int
          | Seconds of int
          | Milliseconds of int
let bigPartString p = 
  match p with
  | Days 0 -> ""
  | Days 1 -> "a day"
  | Days d -> sprintf "%i days" d
  | Hours 0 -> ""
  | Hours 1 -> "an hour"
  | Hours h -> sprintf "%i hours" h
  | Minutes 0 -> ""
  | Minutes 1 -> "a minute"
  | Minutes m -> sprintf "%i minutes" m
  | _ -> ""
let smallPartString s m =
  match s, m with
  | Seconds 0, Milliseconds 0  -> ""
  | Seconds 0, Milliseconds ms -> sprintf "%ims" ms
  | Seconds 1, Milliseconds 0  -> sprintf "a second"
  | Seconds s, Milliseconds 0  -> sprintf "%i seconds" s
  | Seconds s, Milliseconds ms -> sprintf "%i.%i seconds" s ms
  | _                          -> ""

let formatTimeSpan (ts:TimeSpan) maxParts =
  let makePart (p, v) = (p v, v)
  let bigParts = 
    [ (Days, ts.Days)
      (Hours, ts.Hours)
      (Minutes, ts.Minutes)
    ]
    |> Seq.map makePart
    |> Seq.skipWhile (snd >> ((>) 0))
  
  let flip f a b = f b a

  bigParts 
  |> Seq.map fst
  |> Seq.map bigPartString 
  |> flip Seq.append [smallPartString (Seconds ts.Seconds) (Milliseconds ts.Milliseconds)]
  |> Seq.filter (not << String.IsNullOrEmpty)
  |> Seq.truncate maxParts
  |> fun parts -> String.Join(", ", parts)

let ts1 = TimeSpan.FromMinutes(20000.50354)
let ts2 = TimeSpan.FromMinutes(1.5)
let ts3 = TimeSpan.FromSeconds(10.522525)
let ts4 = TimeSpan.FromSeconds(0.0052)

for t in [ts1; ts2; ts3; ts4] do
  printfn "----------"
  for m in [-1..7] do  
    printfn "%s" (formatTimeSpan t m)