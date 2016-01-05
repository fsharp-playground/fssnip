//[snippet:Hex dump of byte array b, display width w.]
// Without ASCII column.

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

(*[omit:(Formatting helper functions omitted.)]*)
// Filters non-printable characters.
let filterChar (b : byte) = if int b <= 32 || int b > 127 then '.' else char b

// Pads a string based on number (w) of bytes to display.
let pad w (s : string) = 
    let t = (w * 2) + w
    if s.Length < t then s + String.replicate (t - s.Length) " " else s (*[/omit]*)

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

// Example output in FSI:
//> hexdump 16 someBytes;;
//00000000  92 36 81 DC 1F F0 87 1F 49 93 8F D3 6C 31 22 71 
//00000010  4F 2E BE C5 8D 5D D1 E6 0F 6A 2B D5 4A 8C 53 B7 
//00000020  01 69 E7 
//val it : unit = ()
//
//> hexdump' 16 someBytes;;
//00000000  49 AA 22 B2 F5 19 FC 48 C5 F9 1F 96 3C C7 53 C8  I."....H....<.S.
//00000010  75 4E 1E 3B 1F 6E CD 68 AB F6 63 FC 80 25 55 A5  uN.;.n.h..c..%U.
//00000020  6C F3 8E 2E FB E3 A4 D3 21 E7 EE 50 0B 1F 9C     l.......!..P...
//val it : unit = ()
//[/snippet]
    

     
