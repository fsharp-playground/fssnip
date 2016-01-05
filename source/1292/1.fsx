module Seq =
    open System.Collections
    let ofBitArray (bitArray:BitArray) = seq { 
        for i=0 to bitArray.Length-1 do yield bitArray.Get(i) 
    }