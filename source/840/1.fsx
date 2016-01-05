(* Affine Cipher - origional in python from http://inventwithpython.com/codebreaker (BSD Licensed) *)
let flip f a b = f b a

let findModInverse a m =
  [0..m] 
  |> Seq.filter (fun b -> a * b % m = 1)  
  |> Seq.head        

let findIndex (s : string) (c : char) =
  s.IndexOf c

let px (s : string) f x g y =
  Seq.map (findIndex s
     >> flip   f x 
     >>        g y 
     >> flip   (%) s.Length
     >> flip Seq.nth s
     >> string)

type Mode = Encrypt | Decrypt

let f M a b =             
  let x = px "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
  let i = findModInverse a 26
  
  match M with
    Encrypt -> x ( * ) a ( + ) b
  | Decrypt -> x ( - ) b ( * ) i

let enc : string -> string = f Encrypt 1 2 >> String.concat ""

let dec : string -> string = f Decrypt 1 2 >> String.concat ""

assert (enc "TEST" = "VGUV")
assert (dec (enc "TEST") = "TEST")
