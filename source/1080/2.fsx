//[snippet:Hex dump of byte array b, display width w.]
open System
open System.Text

let filterChar (b : byte) = if int b <= 32 || int b > 127 then '.' else char b
        
let hexdump w (b : byte[]) =
    let format = sprintf "{0:X8}  {1,-%i} {2}" (w * 2 + w)

    let mapj (sb : StringBuilder * StringBuilder) x = 
        (fst sb).AppendFormat("{0:X2} ", x :> obj), 
        (snd sb).Append(filterChar x)

    seq { for i in 0 .. w .. (b.Length - 1) ->
            let (hex, asc) = 
                Array.sub b i (min w (b.Length - i))
                |> Array.fold (mapj) (StringBuilder(), StringBuilder())
            String.Format(format, i, hex, asc)
        }

//> [|0uy..100uy|] |> hexdump 16 |> Seq.iter (printfn "%s");;
//00000000  00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F  ................
//00000010  10 11 12 13 14 15 16 17 18 19 1A 1B 1C 1D 1E 1F  ................
//00000020  20 21 22 23 24 25 26 27 28 29 2A 2B 2C 2D 2E 2F  .!"#$%&'()*+,-./
//00000030  30 31 32 33 34 35 36 37 38 39 3A 3B 3C 3D 3E 3F  0123456789:;<=>?
//00000040  40 41 42 43 44 45 46 47 48 49 4A 4B 4C 4D 4E 4F  @ABCDEFGHIJKLMNO
//00000050  50 51 52 53 54 55 56 57 58 59 5A 5B 5C 5D 5E 5F  PQRSTUVWXYZ[\]^_
//00000060  60 61 62 63 64                                   `abcd
//Real: 00:00:00.013, CPU: 00:00:00.015, GC gen0: 0, gen1: 0, gen2: 0        

//> Array.zeroCreate 1000000
//|> hexdump 16
//|> Seq.iter ignore;;
//Real: 00:00:00.834, CPU: 00:00:00.842, GC gen0: 875, gen1: 2, gen2: 1        

(*[omit:(Naive slow version omitted)]*)
// Slow version.
(*
let hexdump w (b : byte[]) =
    seq {
        for i in 0..w..b.Length - 1 ->
            let hex = 
                Seq.skip i b 
                |> Seq.truncate w
                |> Seq.map (sprintf "%02X ")
                |> Seq.fold ( + ) ""
            sprintf "%08X  %s\n" i hex  
        }
    |> Seq.iter (printf "%s")

// Filters non-printable characters.
let filterChar (b : byte) = if int b <= 32 || int b > 127 then '.' else char b

// Pads a string based on number (w) of bytes to display.
let pad w (s : string) = 
    let t = (w * 2) + w
    if s.Length < t then s + String.replicate (t - s.Length) " " else s

// With ASCII column.

let hexdump' w (b : byte[]) =
    seq {
        for i in 0..w..b.Length - 1 ->
            let byt = Seq.skip i b 
                      |> Seq.truncate w
            let hex = Seq.map (sprintf "%02X ") byt
                      |> Seq.fold ( + ) "" 
                      |> pad w
            let asc = Seq.map (fun c -> sprintf "%c" (filterChar c)) byt 
                      |> Seq.fold ( + ) "" 
            sprintf "%08X  %s %s\n" i hex asc   
        }
    |> Seq.iter (printf "%s")
*)
(*[/omit]*)
//[/snippet]
    

     