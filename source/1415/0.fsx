open FSharpx.TimeMeasurement

compareTwoRuntimes 
    10
    "sprintf"
    (fun _ -> for _ in 1..100000 do sprintf "%d" 1024 |> ignore)
    "String.Format"
    (fun _ -> for _ in 1..100000 do System.String.Format ("{0}", 1024) |> ignore)

compareTwoRuntimes 
    10
    "sprintf"
    (fun _ -> 
        for _ in 1..100000 do
            sprintf "%d %s %O" 1024 "a string" (obj()) |> ignore)
    "String.Format"
    (fun _ ->
        for _ in 1..100000 do
            System.String.Format ("{0} {1} {2}", 1024, "a string", obj()) |> ignore)

compareTwoRuntimes 
    10
    "sprintf"
    (fun _ -> 
        for _ in 1..100000 do
            sprintf "%d %s %O %d %s %O" 1024 "a string" (obj()) 1024 "a string" (obj()) |> ignore)
    "String.Format"
    (fun _ ->
        for _ in 1..100000 do
            System.String.Format ("{0} {1} {2} {3} {4} {5}", 1024, "a string", obj(), 1024, "a string", obj()) |> ignore)

(*
sprintf 45.0ms
String.Format 25.6ms
  Ratio:  1.7578125

sprintf 165.9ms
String.Format 40.7ms
  Ratio:  4.076167076

sprintf 299.1ms
String.Format 73.7ms
  Ratio:  4.05834464
*)