//#I @"C:\Projects\lib\FSharp.Data"
//#r "FSharp.Data"

open System.Security.Cryptography
open FSharp.Data
open System.Text

//Get your public and private key from http://developer.marvel.com/ 
[<Literal>]
let publicKey = ""
[<Literal>]
let privateKey = ""
[<Literal>]
let characters = "http://gateway.marvel.com/v1/public/characters"

let md5string (value: string) =
    use x = MD5.Create()
    Encoding.UTF8.GetBytes value
    |> x.ComputeHash
    |> Seq.map (fun x -> x.ToString("x2"))
    |> String.concat ""
    
let marvelHash =
    let seed = ref 0
    fun () ->
        incr seed
        let result = sprintf "%i%s%s" !seed privateKey publicKey |> md5string
        !seed, result

//marvelHash() to generate an initial hash

[<Literal>]
let sample = characters + "?ts=1&apikey=" + publicKey + "&hash=<your inital hash here>"

type Characters = JsonProvider<sample>

let result =
    let seed, hash = marvelHash()
    sprintf "%s?ts=%i&apikey=%s&hash=%s" characters seed publicKey hash
    |> Characters.Load

result.Data.Results
|> Array.map (fun x -> x.Name)