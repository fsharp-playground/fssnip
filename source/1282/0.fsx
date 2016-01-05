open Undertone
open Undertone.Waves

let bpm = 90.
let crotchet = Time.noteValue bpm Time.crotchet
let quaver = Time.noteValue bpm Time.quaver

let tune = "C C G G A A AA G F F E E D D C F F F E E D D D C F F F E E E E D D D C C C G G A A AA G F F E E D D C "

let aNote octave length note =
    Creation.makeNote Creation.sine length note octave

let (|Crotchet'|Quaver'|) (str : string) =
    if str.[1] = ' ' then
        Quaver'
    else
        Crotchet'

let LetterToNote (letter : char) =
    printfn "%A" letter
    Note.Parse(typeof<Note>, string letter) :?> Note

let rec tuneToSeq octave str =
    seq {
        match str with
        | Crotchet' ->
            yield! aNote octave crotchet (LetterToNote str.[0])
            let rest =
                str.[3..]
            if String.length rest > 0 then
                yield! tuneToSeq octave rest
        | Quaver' ->
            yield! aNote 4 quaver (LetterToNote str.[0])
            let rest =
                str.[2..]
            if String.length rest > 0 then
                yield! tuneToSeq octave rest }

tuneToSeq 4 tune
|> Player.Play
