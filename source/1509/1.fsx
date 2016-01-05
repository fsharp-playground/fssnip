// Generates strings that are similar to the input, as measured by the
// probability of a character depending on the previous one. (Markov chain)

let readMap (s : string) =
    s |> Seq.windowed 3 |> Seq.groupBy (fun a -> sprintf "%c%c" a.[0] a.[1])
    |> Seq.map (fun (a, b) ->
        let counted = b |> Seq.map (fun arr -> arr.[2])
                        |> Seq.countBy id |> Seq.toList
        let total = List.sumBy snd counted
        a, counted |> List.map (fun (c, i) -> c, float i / float total))
    |> Map.ofSeq

// System.Random is broken. Replace it if you want reliable randomness.
let random = let r = System.Random() in fun () -> r.NextDouble()

let getChar prev cases =
    let rec run r = function
        | [] -> failwith "getChar error"
        | (c, p) :: t when r > p -> run (r-p) t
        | (c, _) :: _ -> c
    run (random() * 0.999) cases // precision safety

let rec generate length acc map =
    if length < 1 then acc |> List.rev |> List.fold (sprintf "%s%c") "" else
    let acc' =
        match acc with
        | h2 :: h1 :: t when Map.containsKey (sprintf "%c%c" h1 h2) map ->
            let key = sprintf "%c%c" h1 h2
            getChar key (map.[key]) :: acc
        | _ -> ' ' :: acc
    generate (length - 1) acc' map

/// duplicate spaces for generation so that words are independent
let wordwise length input =
    let out = (" " + input).Replace(" ", "  ")
              |> readMap |> generate length [' '; ' ']
    printfn "%s" (out.Replace("  ", " "))


// Samples (input a long list of names to get more useful results):

"As I am a monoglot, I do not generally attempt to design languages for other races\
all the writing will be in English. It should be assumed I do not have in mind what\
any particular culture’s language sounds like."
|> wordwise 100

"lololololol zomg roflmao"
|> wordwise 60