let bytesToBits (bytes:byte[]) bitCount =
    let numBytesToFetch = (bitCount - 1) / 8
    let bytes = bytes.[0..numBytesToFetch]
    let bitMasks = Seq.unfold (fun bitIndex -> Some((byte(pown 2 bitIndex), bitIndex), bitIndex + 1)) 0     
                        |> Seq.take 8
                        |> Seq.toList
                        |> List.rev

    let byteToBitArray b = List.map (fun (bitMask, bitPosition) -> (b &&& bitMask) >>> bitPosition) bitMasks

    bytes
        |> Array.toList
        |> List.map byteToBitArray
        |> List.collect id
        |> List.toArray
        |> fun i -> i.[0..bitCount - 1]
