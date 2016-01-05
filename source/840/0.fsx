(* Affine Cipher - origional in python from http://inventwithpython.com/codebreaker (BSD Licensed) *)

let findModInverse a m =
  [0..m] 
  |> Seq.filter (fun b -> a * b % m = 1)  
  |> Seq.head        

let flip f a b = f b a

let p a b (message : string) (S : string) f d f' d' =
  
  message 
  |> Seq.map (fun c -> 
     S.[ S.IndexOf c 
         |> flip f  d 
         |>      f' d' 
         |> flip (%) S.Length]
         |> string)
  |> String.concat ""
  
let F M a b m =             
  let S = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
  let x = p a b m S
  let i = findModInverse a S.Length
  
  match M with
  | "encrypt" -> x ( * ) a ( + ) b
  | "decrypt" -> x ( - ) b ( * ) i
  | _       -> "Invalid Mode, modes = encrypt/decrypt"

(* constraints: 
  a <> 1, 
  b <> 0, 
  gcd a 26 = 1,   
  m is uppercase or S is lowercase.
*)

F "encrypt" 1 2 "TEST" = "VGUV"
F "decrypt" 1 2 (F "encrypt" 1 2 "TEST") = "TEST"
 