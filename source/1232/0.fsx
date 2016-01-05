let chars = [|
    [|0b01110;0b11110;0b01111;0b11110;0b11111;0b11111;0b01111;0b10001;0b01110;0b11111;0b10001;0b10000;0b10001;0b10001;0b01110;0b11110;0b01110;0b11110;0b01111;0b11111;0b10001;0b10001;0b10001;0b10001;0b10001;0b11111|]
    [|0b10001;0b10001;0b10000;0b10001;0b10000;0b10000;0b10000;0b10001;0b00100;0b00001;0b10010;0b10000;0b11011;0b11001;0b10001;0b10001;0b10001;0b10001;0b10000;0b00100;0b10001;0b10001;0b10001;0b01010;0b01010;0b00010|]
    [|0b11111;0b11110;0b10000;0b10001;0b11110;0b11110;0b10011;0b11111;0b00100;0b00001;0b11100;0b10000;0b10101;0b10101;0b10001;0b11110;0b10001;0b11110;0b01110;0b00100;0b10001;0b10001;0b10101;0b00100;0b00100;0b00100|]
    [|0b10001;0b10001;0b10000;0b10001;0b10000;0b10000;0b10001;0b10001;0b00100;0b10001;0b10010;0b10000;0b10001;0b10011;0b10001;0b10000;0b10010;0b10001;0b00001;0b00100;0b10001;0b01010;0b10101;0b01010;0b00100;0b01000|]
    [|0b10001;0b11110;0b01111;0b11110;0b11111;0b10000;0b01110;0b10001;0b01110;0b01110;0b10001;0b11111;0b10001;0b10001;0b01110;0b10000;0b01101;0b10001;0b11110;0b00100;0b01110;0b00100;0b01010;0b10001;0b00100;0b11111|]
    |]

let toAsciiArt (s:string) =
    let sb = System.Text.StringBuilder()
    let unpack c bits =
        for x = 0 to 4 do               
            if bits &&& (1 <<< (4-x)) <> 0 then c else ' '
            |> sb.Append |> ignore                            
    sb.AppendLine() |> ignore
    for line in chars do
        for i = 0 to s.Length-1 do
            let c = s.[i]
            if c >= 'A' && c <= 'Z' 
            then line.[int c - int 'A'] |> unpack c
            else sb.Append("    ") |> ignore
            sb.Append(' ') |> ignore
        sb.AppendLine() |> ignore
    sb.ToString()

System.String([|'A'..'Z'|]) |> toAsciiArt

"HELLO WORLD" |> toAsciiArt
(*
H   H EEEEE L     L      OOO       W   W  OOO  RRRR  L     DDDD  
H   H E     L     L     O   O      W   W O   O R   R L     D   D 
HHHHH EEEE  L     L     O   O      W W W O   O RRRR  L     D   D 
H   H E     L     L     O   O      W W W O   O R   R L     D   D 
H   H EEEEE LLLLL LLLLL  OOO        W W   OOO  R   R LLLLL DDDD 
*)