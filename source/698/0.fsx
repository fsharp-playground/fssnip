open System
open System.Net

// Discrete Frechet Distance: 
// (Based on the 1994 algorithm from Thomas Eiter and Heikki Mannila.)
let frechet (P : array<float*float>) (Q : array<float*float>) =
    let sq (x : float) = x * x
    let min3 x y z = [x; y; z] |> List.min
    let d (a : float*float) (b: float*float) =
        let ab_x = fst(a) - fst(b)
        let ab_y = snd(a) - snd(b)
        sqrt(sq ab_x + sq ab_y)

    let p, q = Array.length P, Array.length Q
    let ca = Array2D.init p q (fun _ _ -> -1.0)

    let rec c i j =
        if ca.[i, j] > -1.0 then
            ca.[i, j]
        else
            if i = 0 && j = 0 then
                ca.[i, j] <- d (P.[0]) (Q.[0])
            elif i > 0 && j = 0 then 
                ca.[i, j] <- Math.Max((c (i-1) 0), (d P.[i] Q.[0]))
            elif i = 0 && j > 0 then 
                ca.[i, j] <- Math.Max((c 0 (j-1)), (d P.[0] Q.[j]))
            elif i > 0 && j > 0 then
                ca.[i, j] <- Math.Max(min3 (c (i-1) j) (c (i-1) (j-1)) (c i (j-1)), (d P.[i] Q.[j]))
            else
                ca.[i, j] <- nan
            ca.[i, j]
    c (p-1) (q-1)

// Use frechet as an operator:
let (-~~) a1 a2 = abs(frechet a1 a2)

// Make an array of letters in roughly sound-alike order, with an index to reflect roughly how
// similar each sounds to its neighbour in the list:
let letters = [|
                    'a', 1.0
                    'e', 2.0
                    'i', 3.0
                    'o', 4.0
                    'u', 5.0
                    'y', 6.0
                    'h', 7.0

                    'b', 17.0
                    'p', 18.0
                    't', 19.0
                    'd', 20.0
                    'j', 21.0

                    'r', 25.0

                    'c', 35.0
                    'k', 36.0
                    'q', 37.0
                    'x', 38.0
                    'g', 39.0

                    'l', 49.0
                    'm', 50.0
                    'n', 51.0

                    's', 61.0
                    'z', 62.0

                    'f', 72.0
                    'v', 73.0

                    'w', 83.0
              |]

// Get the 'similarity index' of a letter:
let letterValue letter = 
    let pair = try
                   letters 
                   |> Array.find (fun elem -> fst(elem) = letter)
               with
                | _ -> ' ', 99.9
    snd(pair)

// Treat a word as a curve of similarity indices:
let wordCurve (word : string) =
    word.ToLower().ToCharArray()
    |> Array.mapi (fun i letter -> float(i), letterValue letter)

// Work out the Frechet distance between two word curves:
let wordDistance word1 word2  =
    (wordCurve word1) -~~ (wordCurve word2)

// Make a funky operator for wordDistance:
let (<-->) = wordDistance

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

// Read a word list and break it up into non-trivial, lowercase words:
let longWordList() =
    let goodLetters = [|'a'..'z'|]
    let goodWord (word : string) =
        let badIndices =
            word.ToCharArray()
            |> Array.map (fun letter -> Array.IndexOf(goodLetters, letter)) 
            |> Array.filter (fun index -> index < 0)
        (badIndices |> Array.length) = 0

    let reader = urlReader "http://unix-tree.huihoo.org/V7/usr/dict/words.html"
    seq {
            while not (reader.EndOfStream) do
            yield (reader.ReadLine().ToLower())
    } 
    |> Seq.skip 17 // Skip HTML
    |> Seq.filter (fun word -> word.Length > 2)
    |> Seq.filter (fun word -> goodWord word)
    |> Seq.cache

let mostLike word n =
    longWordList()
    |> Seq.sortBy (fun listWord -> listWord <--> word)
    |> Seq.truncate n

let leastLike word n =
    longWordList()
    |> Seq.sortBy (fun listWord -> -(listWord <--> word))
    |> Seq.truncate n

// Examples:

//    mostLike "turtle" 10 |> Seq.iter (fun item -> printfn "%s" item);;
//    turtle
//    purple
//    diddle
//    dudley
//    paddle
//    peddle
//    piddle
//    puddle
//    puddly
//    toddle

//    leastLike "turtle" 10 |> Seq.iter (fun item -> printfn "%s" item);;
//    bartholomew
//    counterflow
//    grandnephew
//    hereinbelow
//    marshmallow
//    longfellow
//    afterglow
//    bmw
//    bow
//    caw