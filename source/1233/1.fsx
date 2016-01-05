open System
open FsCheck
open Swensen.Unquote

let byteflip n =
    let rec loop n mask acc =
        match mask > 0x00uy with
        | true ->
            let acc = acc >>> 1
            match n &&& mask with
            | 0x00uy -> loop n (mask >>> 1) acc
            | _      -> loop n (mask >>> 1) (acc ||| 0x80uy)
        | false -> acc
    loop n 0x80uy 0x00uy

test <@ byteflip 0b0001101uy = 0b10110000uy @>
test <@ byteflip 0b0000001uy = 0b10000000uy @>
Check.Quick <| fun n -> n = (byteflip >> byteflip) n