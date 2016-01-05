let loadRawStats (fileName : string) =
(*[omit:(body omitted)]*)
    [for i in 0..1000 -> float(i)]
(*[/omit]*)

let removeOutliers stats =
(*[omit:(body omitted)]*)
    stats
(*[/omit]*)

let normalize stats =
(*[omit:(body omitted)]*)
    stats
(*[/omit]*)

let scale stats =
(*[omit:(body omitted)]*)
    stats
(*[/omit]*)

let transFormStats (fileName : string) = 
    loadRawStats fileName |> removeOutliers |> normalize |> scale
