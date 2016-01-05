type SB = System.Text.StringBuilder

let add10M zero add =
    let mutable state = zero
    for i = 1 to 10000000 do
        state <- add state "a"
    state

let testAddBuild reset add build =
    let zero = reset ()
    let t0 = System.DateTime.Now
    let state = add10M zero add
    let t1 = System.DateTime.Now
    build state
    let t2 = System.DateTime.Now
    (t1 - t0).TotalMilliseconds, (t2 - t1).TotalMilliseconds

let sbReset, sbAdd, sbBuild =
    let reset () = SB()
    let add (sb : SB) (s : string) = sb.Append s
    let build sb = sb.ToString() |> ignore
    reset, add, build

let rsReset, rsAdd, rsBuild =
    let reset () = []
    let add t h = h :: t
    let build l = System.String.Join("", List.rev) |> ignore
    reset, add, build

let sbTest () = testAddBuild sbReset sbAdd sbBuild
let rsTest () = testAddBuild rsReset rsAdd rsBuild

let results () =
    let mean test =
        let pairs = [ for i in 1 .. 100 -> printfn "%d" i; test() ]
        (pairs |> List.map fst |> List.sum) / 100.,
        (pairs |> List.map snd |> List.sum) / 100.
    let sbMean = mean sbTest
    let rsMean = mean rsTest
    printfn "SB: %f / %f" (fst sbMean) (snd sbMean)
    printfn "RS: %f / %f" (fst rsMean) (snd rsMean)