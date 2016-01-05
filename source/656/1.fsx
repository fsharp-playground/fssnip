
let isalpha = System.Char.IsLetter
       
(* Only do lowers for now *)
let shift n c = 
   if isalpha c then
     char ((( int c - 97 + n ) % 26 ) + 97)
   else if c = ' ' then ' '
   else c

let encode   xs n = String.map (shift n) xs
let countSum xs c = Seq.fold (fun acc x -> if x = c then acc+1.0 else acc) 0.0 xs

let freqTab word =
  let freq = Seq.map (countSum word) ['a'..'z']
  let sum = Seq.sum freq
  Seq.map (fun c -> c * 100.0 / sum) freq
        
(* 
 X^2 = sum i=1 .. n * (o - e)^2 / e
 Where:
  X^2 - is the test statistic that asymptotically approaches a x 2 (Chi) distribution. 
  o  - an observed frequency
  e  - an expected (theoretical) frequency, asserted by the null hypothesis.
  n  - the number of possible outcomes of each event. 
*)

let chisqr (a : float seq) (b : float seq) =
  Seq.zip a b           
  |> Seq.fold (fun acc (o,e) -> acc+((o - e) * (o - e) / e)) 0.0
  

let freqTable = [ 8.2; 1.5; 2.8; 4.3; 12.7; 2.2; 2.0; 6.1; 7.0;
                  0.2; 0.8; 4.0; 2.4; 6.7;  7.5; 1.9; 0.1; 6.0;
                  6.3; 9.1; 2.8; 1.0; 2.4;  0.2; 2.0; 0.1 ]
     
let crack word =
  
  [0..25] 
  |> Seq.map (fun x -> x, chisqr (freqTab (encode word x)) freqTable)
  |> Seq.minBy (fun (x,y) -> y)
  |> fst 
  |> encode word
  
(* Tests *)
//> let test = encode "hello how are you?" 3;;
//val test : string = "khoor krz duh brx?"

//> crack test;;
//val it : string = "hello how are you?"

