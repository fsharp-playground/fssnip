(* Hackthissite.org Programming level 12. 

Problem text:
This level is about string manipulation.
In this challenge, you will be given a string. 
Take all the numbers from the string and classify them as 
composite numbers or prime numbers. 
You should assume all numbers are one digit, and neither 
number 1 nor number 0 counts. 

Find the sum of every composite number, 
then find the sum of every prime number. 
Multiply these sums together. 
Then, take the first 25 non-numeric characters of 
the given string and increment their ASCII value by one 
(for example, # becomes $). 
Take these 25 characters and concatenate the product to them. 
This is your answer.
Your answer should look like this: oc{lujxdpb%jvqrt{luruudtx140224

Summarised algorithm.
1. extract single numbers from string excluding 1 and 0
2. classify sequence as prime or composite.
3. sum primes and sum composites, multiply result
4. take first 25 non-numeric chars, increment ascii value by 1.
5. concat product from step 3 to the value of step 4.
*)
open System

let isValidNumber d = d > 47 && d < 58

// ensure no 0 or 1
let extractNumbers (str : string) = 
  str
  |> Seq.filter (fun x -> Char.IsDigit x && x <> '0' && x <> '1')
  |> Seq.map (string >> int)

let isPrime d =
  [2; 3; 5; 7] |> Seq.exists ((=) d)

let sum f = Seq.filter f >> Seq.sum

let getSumProduct input = 
  let intsOfInput = input       |> extractNumbers  
  let primes      = intsOfInput |> sum isPrime   
  let compos      = intsOfInput |> sum (not << isPrime)  
  
  primes * compos 

let first25Add (xs : string) = 
  xs |> Seq.filter (int >> isValidNumber >> not)
     |> Seq.take 25
     |> Seq.map (int >> (+) 1 >> char >> string)
     |> String.concat ""

let generateAnswer input = 
  first25Add input ^ (getSumProduct input |> string)
  |> System.Windows.Forms.Clipboard.SetText