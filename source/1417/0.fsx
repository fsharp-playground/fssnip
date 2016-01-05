open FSharpx.TimeMeasurement
open TypeSet.Provided

printfn "Single Int:"
compareTwoRuntimes 
    10
    "sprintf"
    (fun _ -> for _ in 1..100000 do sprintf "%d" 1024 |> ignore)
    "String.Format"
    (fun _ -> for _ in 1..100000 do System.String.Format ("{0}", 1024) |> ignore)

compareTwoRuntimes 
    10
    "sprintf"
    (fun _ -> for _ in 1..100000 do sprintf "%d" 1024 |> ignore)
    "TypeSet"
    (fun _ -> for _ in 1..100000 do TPrint<"%d">.show 1024 |> ignore)

compareTwoRuntimes 
    10
    "TypeSet"
    (fun _ -> for _ in 1..100000 do TPrint<"%d">.show 1024 |> ignore)
    "String.Format"
    (fun _ -> for _ in 1..100000 do System.String.Format ("{0}", 1024) |> ignore)


printfn ""
printfn ""


printfn "Int and String"

compareTwoRuntimes 
    10
    "TPrint"
    (fun _ -> 
        for _ in 1..100000 do
            TPrint<"%d %s %s">.show 1024 "a string" "Another" |> ignore)
    "String.Format"
    (fun _ ->
        for _ in 1..100000 do
            System.String.Format ("{0} {1} {2}", 1024, "a string", "Another") |> ignore)


printfn ""
printfn ""

printfn "A bit longer string"

compareTwoRuntimes 
    10
    "sprintf"
    (fun _ -> 
        for _ in 1..100000 do
            TPrint<"%d %s %s %d %s %s">.show 1024 "a string" "#" 1024 "a string" "Some more" |> ignore)
    "String.Format"
    (fun _ ->
        for _ in 1..100000 do
            System.String.Format ("{0} {1} {2} {3} {4} {5}", 1024, "a string", "#", 1024, "a string", "Some more") |> ignore)


System.Console.ReadLine()|>ignore
(*
Single Int:
sprintf 54.5ms
String.Format 25.9ms
  Ratio:  2.104247104
sprintf 49.4ms
TypeSet 18.5ms
  Ratio:  2.67027027
TypeSet 20.4ms
String.Format 25.9ms
  Ratio:  0.7876447876


Int and String
TPrint 33.7ms
String.Format 33.8ms
  Ratio:  0.9970414201


A bit longer string
sprintf 71.9ms
String.Format 72.0ms
  Ratio:  0.9986111111
*)