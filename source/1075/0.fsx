open System.Security.Cryptography
open System.Text
open System.IO

let (bitMap:byte[]) = Array.zeroCreate 20000000  
    
let convertToNumber byte1 byte2 byte3 = 
    int (int byte1 + ((int byte2) <<< 8) + ((int byte3) <<< 16)) 

let createHash (word : string) = 
    let ts = MD5.Create()
    let hash:byte[] = System.Text.Encoding.ASCII.GetBytes(word)
                     |> ts.ComputeHash 
    let a = convertToNumber hash.[0] hash.[1] hash.[8]
    let b = convertToNumber hash.[2] hash.[3] hash.[9]
    let c = convertToNumber hash.[4] hash.[5] hash.[10]
    let d = convertToNumber hash.[6] hash.[7] hash.[11]
    (int a, int b, int c, int d)

let setBit (pos) = 
    bitMap.[pos/8] <- byte (bitMap.[pos/8] ||| (1uy <<< (pos % 8)))

let getBit (pos) = 
    byte ((bitMap.[pos/8] &&& (1uy <<< (pos % 8))) >>> (pos % 8))
    
let addWord (word : string) = 
    createHash word
    |> (fun (a, b, c, d) -> setBit a; setBit b; setBit c; setBit d)

let isWordInDictionary (word : string) = 
    let (p, q, r, s) =
        createHash word
        |> (fun (a, b, c, d) -> (getBit a, getBit b, getBit c, getBit d))   
    (p &&& q &&& r &&& s = 1uy)

let readDictionary = 
   File.ReadLines @"wordlist.txt"
    |> Seq.iter(fun w -> addWord w )

isWordInDictionary "Zulu"
