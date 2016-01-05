let flip f a b = f b a

let findModInverse a m =
  [0..m] 
  |> Seq.filter (fun b -> a * b % m = 1)  
  |> Seq.head        

let findIndex (s : string) (c : char) =
  s.IndexOf c

let pxn s f = 
  Seq.map (findIndex s >> f >> flip Seq.nth s >> string)

let encode : (int -> int) -> (char seq -> string seq) = 
  pxn "ABCDEFGHIJKLMNOPQRSTUVWXYZ"

let enc a b = 
  encode (fun n -> a * n + b)

let dec a b = 
  encode (fun n -> n * (findModInverse a 26) - b)

let encrypt : string -> string = enc 1 2 >> String.concat ""
let decrypt : string -> string = dec 1 2 >> String.concat ""

assert (enc "TEST" = "VGUV")
assert (dec (enc "TEST") = "TEST")