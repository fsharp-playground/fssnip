let enumFromByTo from by to' = seq { from .. by .. to'}
let enumFromTo from = enumFromByTo from 1
let enumTo = enumFromTo 1

let mul = (*)

let factorial = enumTo >> Seq.fold mul 1
