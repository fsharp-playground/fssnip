(* Shift cipher aka caesar *)
(* Taken from http://www.paul-abraham.com/CaesarCipher.doc and cleaned up*)
let isAlpha c = System.Char.IsLetter c
       
(* Only do lowers for now *)
let shift n c  = 
  let b = 
    match isAlpha c with
    | true  -> (( int c - 97 + n ) % 26 ) + 97
    | false -> 
      match c with 
      | ' ' -> 32
      | _   -> int c
  char b
                 
let encode xs n = 
  xs |> Seq.map (shift n) 
     |> Seq.toArray
     |> fun e -> new string (e)

let findall xs e = 
  seq { for i in xs -> if i = e then 1 else 0 }

let count xs c = 
  findall xs c |> (float << Seq.fold (+) 0)
               
let freqTab word =
  [ for x in 'a'..'z' -> (count word x) ] 
    |> fun count' ->
       count' 
       |> Seq.map (fun c -> 
          c * 100.0 / (count' |> Seq.sum))
        
(* 
 X^2 = sum i=1 .. n * (Oi - Ei)^2 / Ei
 Where:
  X^2 - is the test statistic that asymptotically approaches a x 2 (Chi) distribution. 
  Oi  - an observed frequency
  Ei  - an expected (theoretical) frequency, asserted by the null hypothesis.
  n   - the number of possible outcomes of each event. 
*)

let chisqr (a : float seq) (b : float seq) =
  Seq.zip a b           
  |> Seq.map (fun (Oi,Ei) -> (Oi - Ei) * (Oi - Ei) / Ei)
  |> Seq.sum
  
let freqTable = [ 8.2; 1.5; 2.8; 4.3; 12.7; 2.2; 2.0; 6.1; 0.2;
                  7.0; 0.8; 4.0; 2.4; 6.7;  7.5; 1.9; 0.1; 6.0;
                  6.3; 9.1; 2.8; 1.0; 2.4;  0.2; 2.0; 0.1 ]
     
let crack word =
  let enc k = 
    encode word k
  
  [0..25] 
  |> Seq.map (fun x -> x, chisqr (freqTab (enc x)) freqTable)
  |> Seq.minBy (fun (x,y) -> y)
  |> fst 
  |> enc
  
(* Tests *)
> let test = encode "hello how are you?" 3;;
val test : string = "khoor krz duh brx?"

> crack test;;
val it : string = "hello how are you?"
