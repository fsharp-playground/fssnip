open System
let log format = Printf.kprintf (sprintf "%A: %s" DateTime.Now) format