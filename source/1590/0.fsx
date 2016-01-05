open System
open System.Security.Cryptography
open System.Text
open System.Threading

let base32alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567" |> Seq.toArray
let valToChar (b : byte) = base32alphabet.[int (b)]

let groupsOfAtMost (size:int) =
    Seq.mapi (fun i x -> i/size, x)
    >> Seq.groupBy (fun (i,_) -> i)
    >> Seq.map (fun (_,vs) -> vs |> Seq.map (fun (_,b) -> b) |> Seq.toList)

let base32encode (data : byte []) = 
    let cvt = 
        data
        |> groupsOfAtMost 5
        |> Seq.map (fun x -> 
               match x with
               | [ a; b; c; d; e ] -> (a, b, c, d, e)
               | [ a; b; c; d ] -> (a, b, c, d, 0uy)
               | [ a; b; c ] -> (a, b, c, 0uy, 0uy)
               | [ a; b ] -> (a, b, 0uy, 0uy, 0uy)
               | [ a ] -> (a, 0uy, 0uy, 0uy, 0uy)
               | _ -> (0uy, 0uy, 0uy, 0uy, 0uy))
        |> Seq.map (fun (c1, c2, c3, c4, c5) -> 
               [ valToChar (c1 >>> 3)
                 valToChar (((c1 &&& 0b111uy) <<< 2) ||| (c2 >>> 6))
                 valToChar (((c2 &&& 0b111110uy) >>> 1))
                 valToChar (((c2 &&& 0b1uy) <<< 4) ||| (c3 >>> 4))
                 valToChar (((c3 &&& 0b1111uy) <<< 1) ||| (c4 >>> 7))
                 valToChar (((c4 &&& 0b1111100uy) >>> 2))
                 valToChar (((c4 &&& 0b11uy) <<< 3) ||| (c5 >>> 5))
                 valToChar (((c5 &&& 0b11111uy))) ])
        |> Seq.concat
        |> Seq.toArray
    new String(cvt)

let truncate(data : byte []) : uint32 = 
    let offset = int ( Seq.last(data) &&& 0xfuy)
    ((uint32(data.[offset+0] &&& 0x7fuy) <<< 24) |||
     (uint32(data.[offset+1] &&& 0xffuy) <<< 16) |||
     (uint32(data.[offset+2] &&& 0xffuy) <<< 8) |||
     (uint32(data.[offset+3] &&& 0xffuy))) % 1000000ul

let HMAC (K : byte []) (C : byte []) = 
    use hmac = new HMACSHA1(K)
    hmac.ComputeHash C

let counter() = 
    uint64 (Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds)) / 30UL 
    |> BitConverter.GetBytes |> Array.rev
let secret() = "ASDFASDFASDFASDFSADF" |> Encoding.ASCII.GetBytes
let TOTP() = (HMAC (secret()) (counter())) |> truncate

[<EntryPoint>]
let main argv = 
    let p = 
        base32encode (secret())
        |> groupsOfAtMost 3
        |> Seq.map (fun x -> new String(Seq.toArray x) + " ")
        |> Seq.concat
        |> Seq.toArray
    printfn "%s" (new String(p))
    for i in 1..1000 do
        printfn "%06d" (TOTP())
        Thread.Sleep(10 * 1000)
    0
