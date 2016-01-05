open System
open System.Collections.Generic
open System.IO

//create sorted dictionary with each value a set of strings
let dictWords = new SortedDictionary<string, SortedSet<string>>()

//create word key - lower case, sorted characters
let createWordKey (word : string) = 
    (word.ToLower()).ToCharArray()
    |> Array.sort 
    |> String.Concat

let addWordToDict word =
    let u =  createWordKey word
    if dictWords.ContainsKey u then
        if (dictWords.[u].Contains word) = false then
            dictWords.[u].Add (word) |> ignore
    else
        let set = new SortedSet<string>()
        set.Add word |> ignore
        dictWords.[u] <- set

//create sorted dictionary
File.ReadLines "wordlist.txt"
|> Seq.iter (fun w -> addWordToDict w)

//Print anagrams
for key in dictWords.Keys do
    if dictWords.[key].Count > 1 then
        printf "%s : %d " key dictWords.[key].Count
        for l in dictWords.[key] do
            printf "%s " l
        printf "\n "

