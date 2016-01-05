open Deedle

// ----------------------------------------------------------------------------
// Merging 1 .. 8 ordered series

let s1 = series [ for i in 0000001 .. 0300000 -> i => float i ]
let s2 = series [ for i in 1000001 .. 1300000 -> i => float i ]
let s3 = series [ for i in 2000001 .. 2300000 -> i => float i ]
let s4 = series [ for i in 3000001 .. 3300000 -> i => float i ]
let s5 = series [ for i in 4000001 .. 4300000 -> i => float i ]
let s6 = series [ for i in 5000001 .. 5300000 -> i => float i ]
let s7 = series [ for i in 6000001 .. 6300000 -> i => float i ]
let s8 = series [ for i in 7000001 .. 7300000 -> i => float i ]

// 0.95 ~> 0.25
s1.Append(s2) |> ignore

// 2.44 ~> 0.64
s1.Append(s2).Append(s3) |> ignore
// 0.55
s1.Append(s2, s3) |> ignore

// 4.35 ~> 1.09
s1.Append(s2).Append(s3).Append(s4) |> ignore
// 0.83
s1.Append(s2, s3, s4) |> ignore

// 6.80 ~> 1.78
s1.Append(s2).Append(s3).Append(s4).Append(s5) |> ignore
// 1.06
s1.Append(s2, s3, s4, s5) |> ignore

// 17.5 ~> 4.50
s1.Append(s2).Append(s3).Append(s4).Append(s5).Append(s6).Append(s7).Append(s8) |> ignore
// 2.10
s1.Append(s2, s3, s4, s5, s6, s7, s8) |> ignore

// ----------------------------------------------------------------------------
// Merging 64 ordered series

let ss = 
  [ for i in 1 .. 64 ->
      series [ for j in 1000000*i+1 .. 1000000*i+10000 -> j => float j ] ]

// [too long] ~> 18s
ss |> List.fold (fun (s:Series<_, _>) v -> s.Append(v)) s1 |> ignore
// N/A ~> 2.25
s1.Append(Array.ofSeq ss) |> ignore

// ----------------------------------------------------------------------------
// Merging 1 .. 5 unordered series

let r1 = series [ for i in 0000001 .. 0300000 -> i => float i ] |> Series.rev
let r2 = series [ for i in 1000001 .. 1300000 -> i => float i ] |> Series.rev
let r3 = series [ for i in 2000001 .. 2300000 -> i => float i ] |> Series.rev
let r4 = series [ for i in 3000001 .. 3300000 -> i => float i ] |> Series.rev 
let r5 = series [ for i in 4000001 .. 4300000 -> i => float i ] |> Series.rev
let r6 = series [ for i in 5000001 .. 5300000 -> i => float i ] |> Series.rev

// 1.05 ~> 0.27
r1.Append(r2) |> ignore

// 2.62 ~> 0.75
r1.Append(r2).Append(r3) |> ignore
// 0.50
r1.Append(r2, r3) |> ignore

// 4.82 ~> 1.18
r1.Append(r2).Append(r3).Append(r4) |> ignore
// 0.65
r1.Append(r2, r3, r4) |> ignore

// 7.55 ~> 1.76
r1.Append(r2).Append(r3).Append(r4).Append(r5) |> ignore
// 0.85
r1.Append(r2, r3, r4, r5) |> ignore

// ----------------------------------------------------------------------------

let df1 = Frame.ReadCsv(__SOURCE_DIRECTORY__ + @"\..\..\docs\content\data\stocks\msft.csv")
let df2 = df1 |> Frame.shift 1  // This is terribly slow!

let opens1 = df1?Open
let opens2 = df2?Open

// 0.50 ~> 0.35
(df2 - df1) |> ignore
(df2 + df1) |> ignore
(df2 * df1) |> ignore
(df2 / df1) |> ignore
