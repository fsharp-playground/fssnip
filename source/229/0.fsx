let dl = 9.5 / 11.
let min = 21.5 + dl
let max = 40.5 - dl

let a = [ for z in min .. dl .. max -> z ] // should have 21 elements
let b = a.Length