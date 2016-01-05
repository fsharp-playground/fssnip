open System
open FParsec


  
type ValueFilter = 
  | Value of float 
  | GreaterThan of float 
  | Range of float * float

type SearchFilter = Ram | Weight | Camera | Unknown 

let toLowerCase (str : string) = str.ToLowerInvariant()

let toSearchFilter str = 
  match toLowerCase str with
  | "ram" -> Ram
  | "weight" -> Weight
  | "camera" -> Camera
  | _ -> Unknown

[<EntryPoint>]
let main argv = 
  
  let test p str =
    match run p str with
    | Success (v, _, _) -> printfn "S : %A" v
    | Failure (e,_,_) -> printfn "E : %A" e   


  let psearchFilter str = pstring str |>> toSearchFilter
  let pseperator = pchar ':'
  let pgreaterThan = pchar '>' >>. pfloat |>> GreaterThan
  let prange = pfloat .>> (pchar '-') .>>. pfloat |>> Range
  let pvalue = pfloat |>> Value
  let pmeasure suffix = pstringCI suffix
  let pvalueFilter = (choice [pgreaterThan; (attempt prange); pvalue])
  let pFilter ptype pmeasure'  = 
    ptype .>> pseperator .>>. pvalueFilter .>> pmeasure'

  let pmb = pmeasure "MB"
  let pg = pmeasure "g"
  let ppixel = pmeasure "px"
  let pram = psearchFilter "ram"
  let pweight = psearchFilter "weight"
  let pcamera = psearchFilter "camera"

  let pramFilter = pFilter pram pmb
  let pweightFilter = pFilter pweight pg
  let pcameraFilter = pFilter pcamera ppixel
  let pf = choice [pramFilter;pweightFilter;pcameraFilter]
 
  test (sepBy pf (pchar ';')) "weight:>20g;ram:100-200mb;camera:200px"

  0
