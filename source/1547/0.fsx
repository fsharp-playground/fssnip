/// (Infinite) list of note-frequency -pairs
let tones = 
    let bass = 55.0 
    let note = ["A"; "A#"; "B"; "C"; "C#"; "D"; "D#"; "E"; "F"; "F#"; "G"; "G#"]
    let notes = seq { while true do yield! note }
    let frequency = bass |> Seq.unfold (fun x -> Some (x, x*System.Math.Pow(2.0, 1.0 / 12.0)))
    Seq.zip notes frequency

//let ``guitar open A`` = tones |> Seq.nth 24 // val it : float * string = (220.0, "A")

//let ``A to F#`` = tones |> Seq.skip 36 |> Seq.take 10 |> Seq.toArray
//  [|("A", 440.0); ("A#", 466.1637615); ("B", 493.8833013); ("C", 523.2511306);
//    ("C#", 554.365262); ("D", 587.3295358); ("D#", 622.2539674);
//    ("E", 659.2551138); ("F", 698.4564629); ("F#", 739.9888454)|]
