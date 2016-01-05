open System.Text

let hexDump bytesPerLine (bytes: byte array) =
    let join (xs: string array) = 
        xs |> Array.fold (fun (acc: StringBuilder) x -> acc.Append x) (StringBuilder())
           |> sprintf "%O"

    seq { for i in 0..bytesPerLine..(bytes.Length - 1) ->
            Array.sub bytes i (min bytesPerLine (bytes.Length - i))
            |> Array.map (sprintf "%02X ")
            |> join
            |> sprintf "%08X  %s" i }


let hexDump1 bytesPerLine (bytes: byte array) =
    let inline join (xs: string array) = 
        System.String.Join(" ", xs)

    seq { for i in 0..bytesPerLine..(bytes.Length - 1) ->
            i.ToString("X08") + " " +
            (Array.sub bytes i (min bytesPerLine (bytes.Length - i))
             |> Array.map (fun b -> b.ToString("X2"))
             |> join) }

[|0uy..100uy|]
|> hexDump1 16
|> Seq.iter (printfn "%s")

// 00000000  00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 
// 00000010  10 11 12 13 14 15 16 17 18 19 1A 1B 1C 1D 1E 1F 
// 00000020  20 21 22 23 24 25 26 27 28 29 2A 2B 2C 2D 2E 2F 
// 00000030  30 31 32 33 34 35 36 37 38 39 3A 3B 3C 3D 3E 3F 
// 00000040  40 41 42 43 44 45 46 47 48 49 4A 4B 4C 4D 4E 4F 
// 00000050  50 51 52 53 54 55 56 57 58 59 5A 5B 5C 5D 5E 5F 
// 00000060  60 61 62 63 64 

// C2D T7300, W8 x64, .NET 4.5
// benchmark: Real: 00:00:09.061, CPU: 00:00:08.502, GC gen0: 578, gen1: 1, gen2: 1
Array.zeroCreate 1000000
|> hexDump 16
|> Seq.iter ignore

// benchmark1: Real: 00:00:00.255, CPU: 00:00:00.265, GC gen0: 32, gen1: 0, gen2: 0
Array.zeroCreate 1000000
|> hexDump1 16
|> Seq.iter ignore
