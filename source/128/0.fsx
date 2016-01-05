// rather crude, but only a handful of prime numbers are needed here
let isPrime = function
    | n when n < 2 -> false
    | 2 -> true
    | n -> let upper = int (sqrt (float n))
           [2..upper] |> List.forall (fun d -> n%d <> 0)

let primes = {0 .. System.Int32.MaxValue} |> Seq.filter isPrime

let createLookup alphabet = 
    // you can give characters you don't care about a weight of 1
    let ignoredChars = ['\'';' ';'.';'!';':';';';'?']
    
    let nchars = alphabet |> Seq.length
    Seq.zip alphabet (primes |> Seq.take nchars) 
    |> Seq.append (ignoredChars |> Seq.map (fun c -> (c,1)))
    |> dict

let lookup = ['a'..'z'] |> createLookup
// or if you want to discriminate between upper and lower case
// let lookup = ['A'..'Z'] @ ['a'..'z'] |> createLookup

let computeHash (s:string) =
    s |> Seq.fold (fun acc c -> acc * (bigint (lookup.[c]))) 1I

let isAnagram word1 word2 = 
    (computeHash word1) = (computeHash word2)

// --- example (FSI) ---
//
//> isAnagram "team" "meat";;
//Real: 00:00:00.000, CPU: 00:00:00.000, GC gen0: 0, gen1: 0, gen2: 0
//val it : bool = true
//> isAnagram "team" "meet";;
//Real: 00:00:00.000, CPU: 00:00:00.000, GC gen0: 0, gen1: 0, gen2: 0
//val it : bool = false  
//> isAnagram "crazy dudes" "dude's crazy";;
//Real: 00:00:00.001, CPU: 00:00:00.000, GC gen0: 0, gen1: 0, gen2: 0
//val it : bool = true

// --- another example: let's find a word with the most anagrams ---
// source: http://sourceforge.net/projects/wordlist/files/ [continued] 
//                Ispell-EnWL/3.1.20/ispell-enwl-3.1.20.zip/download
let wordPath = @"c:\temp\words\english.0" 

let readWords path = 
    System.IO.File.ReadAllLines(path)
    |> Array.map (fun word -> word.ToLowerInvariant())
    
let mostAnagrams() = 
    readWords wordPath
    |> Seq.groupBy computeHash
    |> Seq.sortBy (fun (_, anagrams) -> -(Seq.length anagrams))
    |> Seq.head
    |> snd
    |> Seq.toList

let longestWordWithTwoAnagrams() =
    readWords wordPath    
    //cellars and cellar's are the same for this purpose
    |> Seq.distinctBy (fun word -> word.Replace("\'","")) 
    |> Seq.sortBy (fun word -> -(word.Length))
    |> Seq.groupBy computeHash
    |> Seq.filter (fun (_,anagrams) -> Seq.length anagrams = 3)
    |> Seq.head 
    |> snd
    |> Seq.toList

// FSI
//> mostAnagrams();;
//Real: 00:00:00.537, CPU: 00:00:00.531, GC gen0: 9, gen1: 9, gen2: 0
//val it : string list =
//  ["asper"; "pares"; "parse"; "pears"; "rapes"; "reaps"; "spare"; "spear"]
//> longestWordWithTwoAnagrams();;
//Real: 00:00:00.525, CPU: 00:00:00.531, GC gen0: 7, gen1: 7, gen2: 0
//val it : string list = ["reduction's"; "discounter"; "introduces"]    

    
  