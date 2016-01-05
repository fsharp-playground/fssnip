open System
open System.IO
open System.Text

let hexdump (bytes: byte[]) =
    let memStream = new MemoryStream(bytes)
    let buffer = Array.zeroCreate 16
    let read = ref 1
    let totalRead = ref 0
    while !read > 0 do
        read := memStream.Read(buffer, 0, buffer.Length)
        printf "%08x  " !totalRead
        for x in 0 .. 3 do printf "%02x " buffer.[x]
        printf " "
        for x in 4 .. 7 do printf "%02x " buffer.[x]
        let chars = Encoding.ASCII.GetChars(buffer)
        printf "  |"
        for x in chars do
            let printChar =
                if Char.IsControl(x) then
                    '.'
                else
                    x
            printf "%c" printChar
        printfn "|"
        totalRead := !totalRead + !read