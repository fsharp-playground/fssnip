let voltagesEU = [|0.; -325.; 0.; 325.|]
let voltagesUS = [|0.; -170.; 0.; 170.|]

let RMS data =
    data 
    |> Seq.averageBy (fun v -> v ** 2.)
    |> sqrt

do
    printfn "EU: %0.0f volts" (voltagesEU |> RMS) // 230
    printfn "US: %0.0f volts" (voltagesUS |> RMS) // 120