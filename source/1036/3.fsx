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
let isElem  x e  = Seq.exists ((=) e) x
let isPrime      = isElem [2; 3; 5; 7]  
let sumOf f : int seq -> int = Seq.filter f >> Seq.sum

let getSumProduct xs = 
  let extractNumbers =
    Seq.filter (isElem "23456789") >> Seq.map (string >> int)

  let intxs  =  extractNumbers xs  
  let primes = intxs |> sumOf isPrime   
  let compos = intxs |> sumOf (not << isPrime)  
  
  primes * compos 

let toString : char seq -> string = 
  Seq.map string >> String.concat ""

let incrChar (x : char) = char (1 + int x)

let take25NonIntsIncrByOne xs = 
  xs 
  |> Seq.filter (not << isElem "0123456789") 
  |> Seq.take 25
  |> Seq.map incrChar
  |> toString

let generateAnswer input = 
  take25NonIntsIncrByOne input ^ (getSumProduct input |> string)
  |> System.Windows.Forms.Clipboard.SetText

let input = "@yfe1ihmjbm@4shk$@jn8i5udt@w3ytg9$adra293ll@#1$boxeust8fe56ra41fu$ckfo9cuic0nk9c2xp2qmt$8d#bksgtuvss4tmw8bs91axn7gbyuz1kwewon0matt?rnk0q1jl3ml7tcya0pmir13ekm@z@v#uonxfo?vxa6d@#eg09rqlkqc3vwb#jcshizhh5w83jc9czsljzji46dapl$h8g#rny1lwcddh8m07xt8s$xngvzg2rwnedz1xxa2rey4t@4fvj0m?#l1rkf1@ox#2rsbpt7hdftltp9om26kd@xb@tvet25$j1f5dx3pfur$e3asyzv0tm9rseu2iogusr0z#hjyui35i4oeu?1qu8nio@tmgv7qigar5j#1mx707mxobh23i7wp#fn77f#bry$@#20h4w31bdnhs5woyupo3h@8gd8ukoqjnn41q9ekjy07fp4v761wqkck7ex#$q$gm82o#tmh3c7#1ju@j9p2t3m#qp5o4hnmy5enye48om?z5#hfhbc05yugmx57j3n3$gi?0rebwxdehtc$e431ldvy?v1xtej3mxxploweuyprfwnn0@#6l?tyss2m@8ge9rgq@#o0g5@s@ox7cdz181tcq4?f#i"

input |> generateAnswer