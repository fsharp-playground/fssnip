let Crc16 msg =
    let polynomial      = 0xA001us
    let mutable code    = 0xffffus
    for b in msg do
        code <- code ^^^ uint16 b
        for j in [0..7] do
            if (code &&& 1us <> 0us) then
                code <- (code >>> 1) ^^^ polynomial
            else
                code <- code >>> 1
    code