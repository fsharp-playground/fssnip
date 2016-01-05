// Generates strings that are similar to the input, as measured by the
// probability of a character depending on the previous one. (Markov chain)

let readMap (s : string) =
    s |> Seq.windowed 2 |> Seq.groupBy (fun arr -> arr.[0])
    |> Seq.map (fun (a, b) ->
        let counted = b |> Seq.map (fun arr -> arr.[1])
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
    let acc' = match acc with
               | h :: t when Map.containsKey h map ->
                   getChar h (map.[h]) :: acc
               | _ -> ' ' :: acc
    generate (length - 1) acc' map


// Example use

let test firstChar length input =
    readMap input |> generate length [firstChar] |> printfn "%s"

"cate kristen julia sarah amy natalie lisa beatrice"
|> test ' ' 80

"As I am a monoglot, I do not generally attempt to design languages for other races\
all the writing will be in English. It should be assumed I do not have in mind what\
any particular culture’s language sounds like."
|> test ' ' 100

"lololololol zomg roflmao"
|> test 'z' 60