let gfailwith (exncons : string -> exn) (msg : string) = exncons msg |> raise
let gfailwithf (exncons : string -> exn) fmt = Printf.ksprintf (gfailwith exncons) fmt

// example

open System

let failwithf fmt = gfailwithf (fun msg -> new ArgumentException(msg) :> exn) fmt

let rec factorial =
    function
    | n when n < 0 -> failwithf "factorial: invalid argument %d." n 
    | 0 -> 1
    | n -> n * factorial (n-1)


factorial -2