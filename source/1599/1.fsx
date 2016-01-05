open System.IO

let getDirectory =
  ("a"
  , "b"
  , "c")
  |> Path.Combine
  |> fun f -> new DirectoryInfo(f)    // this one works
  // |> new DirectoryInfo             // this one doesn't work

getDirectory.FullName

//> val getDirectory : DirectoryInfo = a\b\c
//> val it : string = "C:\Users\Me\AppData\Local\Temp\a\b\c"

(* ideally:

let getDirectory =
  "a"
  , "b"
  , "c"
  |> Path.Combine
  |> new DirectoryInfo
*)
