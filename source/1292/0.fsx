module Seq =
    open System.Collections
    let ofBitArray (bitArray:BitArray) = seq { 
        for i=0 to bitArray.Length do yield bitArray.Get(i) 
    }