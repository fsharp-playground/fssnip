// Generates strings that are similar to the input, as measured by the
// probability of a symbol depending on preceding symbols. (Markov chain)
// The order defines how many preceding symbols to look at to place another.

/// Reads a map of next characters' probabilities from a sample.
let readMap order (s : string) =
    s |> Seq.windowed (order + 1)
    |> Seq.groupBy (fun a -> System.String( a.[0..order - 1] ))
    |> Seq.map (fun (a, b) ->
        let counted = b |> Seq.map (fun arr -> arr.[order])
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
let rec generate order length acc map =
    if length < 1 then acc else
    let acc' =
        if String.length acc < order then sprintf "%s " acc else
        let sub = acc.Substring(acc.Length - order)
        if Map.containsKey sub map then sprintf "%s%c" acc (getChar sub map.[sub])
        else failwith (sprintf "No viable solution in map for %s." sub)
    generate order (length - 1) acc' map

/// Generates words from sample. Generates space-separated words independently.
let wordwise order approxLength input =
    let out = (" " + input + " ").Replace(" ", String.replicate order " ")
              |> readMap order |> generate order approxLength ""
    out.Replace(String.replicate order " ", " ").Remove(0,1) |> printfn "%s"


// Samples (input a long list of names to get more useful results):

let lolz = "lololololol zomg roflmao"
wordwise 1 60 lolz
wordwise 2 60 lolz

"Mercury Venus Earth Mars Asteroid Belt Jupiter Saturn Neptune Pluto Moon Terra Luna \
Adrastea Ganymede Callisto Europa Himalia Amalthea Thebe Elara Metis Pasiphae Carme \
Sinope Lysithea Ananke Leda Themisto Callirrhoe Praxidike Megaclite Locaste Taygete \
Kalyke Autonoe Harpalyke Titan Rhea Iapetus Dione Tethys Enceladus Mimas Hyperion \
Phoebe Janus Epimetheus Prometheus Pandora Titania Oberon Umbriel Ariel Miranda \
Sycorax Puck Portia Juliet Caliban Belinda Cressida Triton Proteus Nereid Larissa \
Galatea Despina Thalassa Charon"
|> wordwise 2 200