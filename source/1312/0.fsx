open System
open System.IO
open System.Text

let hexdump byteCount (bytes: byte[]) =
    let totalLength = Array.length bytes
    let (|Char|Ctrl|) c = char bytes.[c] |> fun ch -> if Char.IsControl(ch) then Ctrl(c) else Char(c,ch) 
    let (|TooLong|_|) c = if c < totalLength then None else Some(c)
    let rec printLine pos =
        if pos < totalLength then
            let length = min byteCount (totalLength - pos)
            let rec printByte =
                function
                | _,0 -> printf "  |"
                | TooLong c, l -> printf "   "; printByte (c+1,l-1)
                | c,l -> printf "%02x " bytes.[c]; printByte (c+1,l-1)

            let rec printChar =
                function
                | _,0 -> printfn "|"
                | TooLong c,l -> printf "."; printChar (c+1,l-1)
                | Ctrl c,l -> printf "."; printChar (c+1,l-1)
                | Char(c,ch),l -> printf "%c" ch; printChar (c+1,l-1)

            printf "%08x  " pos
            printByte (pos, byteCount)
            printChar (pos, byteCount)

            printLine (pos + length)
    printLine 0
hexdump 8 "Hello world\nHow doing ?"B

