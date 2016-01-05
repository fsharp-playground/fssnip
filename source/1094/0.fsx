let numDigits = 3
if (numDigits < 1) then failwith "Number of digits must be at least 1" 

let lowestNumDigitNumber = pown 10 (numDigits - 1)  
let highestNumDigitNumber = (pown 10 numDigits) - 1
let baseSeq = {lowestNumDigitNumber..highestNumDigitNumber}  

let reverse (t : string) =
    new string(t.ToCharArray() |> Array.rev)
    
let isPalindrome t = reverse (string t) = string t

Seq.map (fun x -> (Seq.map (fun y -> x * y) baseSeq)) baseSeq
|> Seq.concat
|> Seq.filter isPalindrome
|> Seq.max
|> printfn "Largest palindrome made from the product of two %d-digit numbers: %A" numDigits