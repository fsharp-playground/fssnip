#if INTERACTIVE
#r ".dll"
#endif

let private main (args: string []) =
    0

#if INTERACTIVE
main (fsi.CommandLineArgs |> Array.toList |> List.tail |> List.toArray) |> ignore
#else
[<EntryPoint>]
let entryPoint args = main args
#endif
