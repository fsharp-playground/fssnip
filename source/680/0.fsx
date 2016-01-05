module CalculatorWords

open System.Net
open System.Collections.Generic

// Basic mappings:
let substitutions = 
    [
        ('O', '0')
        ('I', '1')
        ('Z', '2')
        ('E', '3')
        ('H', '4')
        ('S', '5')
        ('L', '7')
        ('B', '8')
        ('G', '9')
    ]

// Map of mappings:
let subsMap = substitutions |> Map.ofList

// Letters which are in the mappings:
let numberLetters = substitutions |> List.map (fun subs -> fst(subs))

// Read a file from a url but as if it's local for performance:
let urlReader (url : string) =
    let req = WebRequest.Create(url, Timeout = 1000 * 60 * 20)
    try
        let resp = req.GetResponse()
        let stream = resp.GetResponseStream()
        let tempFileName = System.IO.Path.GetTempFileName()
        let tempFileStream = new System.IO.FileStream(tempFileName, System.IO.FileMode.Truncate)
        stream.CopyTo(tempFileStream)
        tempFileStream.Seek(int64(0), System.IO.SeekOrigin.Begin) |> ignore
        new System.IO.StreamReader(tempFileStream)
    with
        | _ as ex -> failwith ex.Message

// Read a word list and break it up into non-trivial, uppercase words:
let words() =
    let reader = urlReader "http://unix-tree.huihoo.org/V7/usr/dict/words.html"
    seq {
            while not (reader.EndOfStream) do
            yield (reader.ReadLine().ToUpper())
    } 
    |> Seq.skip 17 // Skip HTML
    |> Seq.filter (fun word -> word.Length > 2)
    |> Seq.cache

// Check if a word consists of calculator-letters:
let goodWord (word : string) =
    // The word isn't good if it has any letters which aren't one of our 'number' letters:
    word 
    |> String.exists (fun wordLetter -> ( numberLetters 
                                          |> List.tryFindIndex (fun numberLetter -> numberLetter = wordLetter
                                        ) = None))
    |> not

// Translate a word into its calculator-letters equivalent:
let translate (word : string) =
    word
    |> String.map (fun letter -> subsMap.Item letter)
    |> Array.ofSeq
    |> Array.rev
    |> Array.fold (fun acc elem -> sprintf "%s%c" acc elem) ""

// Get all the words which can be spelt on a calculator:
let calculatorWords =
    words()
    |> Seq.filter (fun word -> goodWord word)

// Count the calculator words:
let countWords = 
    calculatorWords |> Seq.length
// 179

// Print calculator words in plain and calculator versions:
let printWords() =
    calculatorWords
    |> Seq.iter (fun word -> printfn "%s (%s)" word (translate word))
// BEE (338)
// BEEBE (38338)
// BEG (938)
// BEIGE (39138)
// BEL (738)
// BELIE (31738)
// BELL (7738)
// ...
// ZIG (912)
// ZOE (302)
// ZOO (002)
 
