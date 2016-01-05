#r "FSharp.PowerPack.dll"

open System
open Microsoft.FSharp.Math

/// convert int to bignum
let toBigNum = bignum.FromInt

/// get integer part of a bignum
let floorBigNum = bignum.ToBigInt>>bignum.FromBigInt

/// convert bignum to string with the specified fraction length
let toString fractionLength (value:bignum) =
  let integer = floorBigNum value
  seq { 
  let fraction = ref ((value - integer) * 10N)
  while !fraction <> 0N do
    let digit = floorBigNum !fraction
    fraction := (!fraction - digit) * 10N
    yield string digit }
  |> Seq.take fractionLength
  |> String.concat ""
  |> fun fraction -> string integer + "." + fraction

/// print bignum
let print len = printfn"%s"<<toString len

/// Isaac Newton 1665–66
let newton n = 
  seq { 
  let a = ref 1N
  let b = ref 2N
  for i in 1 .. n do
    let i = toBigNum i * 2N
    a := !a * (i-1N)
    b := !b * 4N * i
    yield !a / !b / (i+1N)
  }
  |> Seq.sum
  |> (fun v -> v * 6N + 3N)

newton 200 |> print 100
// 3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679

/// calculate arctan value
let arctan n (bn:bignum) =
  seq {
  yield bn
  let a = ref bn
  for i in 1 .. n do
    a := - !a * bn * bn 
    yield !a / (2N * toBigNum i + 1N)
  }
  |> Seq.sum

/// John Machin 1706
let matchin n = 16N * arctan(3*n) (1N/5N) - 4N * arctan n (1N/239N)


matchin 24 |> print 100
// 3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679

/// Ramanujan
let ramanujan n = 
  seq {
    yield 1123N
    let a = ref 1N
    for i in 1 .. n do
      let i = toBigNum i
      a := 
        - !a 
        * List.reduce ( * ) [4N*i-3N..4N*i]
        / BigNum.PowN(i,4) / 199148544N
      yield !a * (21460N * i + 1123N)
  }
  |> Seq.sum
  |> fun x -> 3528N / x

ramanujan 17 |> print 100
// 3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679

let pi1 = newton 200 |> toString 100
let pi2 = matchin 24 |> toString 100
let pi3 = ramanujan 17 |> toString 100

(pi1 = pi2 && pi2 = pi3) |> printfn "%b"
// true