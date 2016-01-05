// Generates strings that are similar to the input, as measured by the
// probability of a character depending on the two previous ones. (Markov chain)

/// Reads a map of next characters' probabilities from a sample.
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

/// Helper to get one character from a list of choices with probabilities
let getChar prev cases =
    let rec run r = function
        | [] -> failwith "getChar error"
        | (c, p) :: t when r > p -> run (r-p) t
        | (c, _) :: _ -> c
    run (random() * 0.999) cases // precision safety

/// Creates text according to a distribution. Fills in spaces if accumulator is empty.
let rec generate length acc map =
    if length < 1 then acc |> List.rev |> List.fold (sprintf "%s%c") "" else
    let acc' =
        match acc with
        | h2 :: h1 :: t when Map.containsKey (sprintf "%c%c" h1 h2) map ->
            let key = sprintf "%c%c" h1 h2
            getChar key (map.[key]) :: acc
        | _ -> ' ' :: acc
    generate (length - 1) acc' map

/// Generates words from sample. Handles space-separated words completely separately.
let wordwise length input =
    let out = (" " + input).Replace(" ", "  ")
              |> readMap |> generate length [' '; ' ']
    out.Replace("  ", " ").Remove(0,1) |> printfn "%s"


// Samples (input a long list of names to get more useful results):

"lololololol zomg roflmao"
|> wordwise 60

"Mercury Venus Earth Mars Asteroid Belt Jupiter Saturn Neptune Pluto Moon Terra Luna \
Adrastea Ganymede Callisto Europa Himalia Amalthea Thebe Elara Metis Pasiphae Carme \
Sinope Lysithea Ananke Leda Themisto Callirrhoe Praxidike Megaclite Locaste Taygete \
Kalyke Autonoe Harpalyke Titan Rhea Iapetus Dione Tethys Enceladus Mimas Hyperion \
Phoebe Janus Epimetheus Prometheus Pandora Titania Oberon Umbriel Ariel Miranda \
Sycorax Puck Portia Juliet Caliban Belinda Cressida Triton Proteus Nereid Larissa \
Galatea Despina Thalassa Charon"
|> wordwise 200