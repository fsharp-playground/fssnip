open System.IO
open Undertone
open Undertone.Waves

//C C G G A A AA G 
//Baa baa black sheep have you any wool?
let bpm = 90.
let crotchet = Time.noteValue bpm Time.crotchet
let quaver = Time.noteValue bpm Time.quaver

let (!+) note = 
    Creation.makeNote Creation.sine quaver note 4
let (!!+) note = 
    Creation.makeNote Creation.sine crotchet note 4

let tune = [!+Note.C;!+Note.C;!+Note.G;!+Note.G;!+Note.A;!+Note.A;!!+Note.A;!+Note.G]
           |> Seq.fold Seq.append Seq.empty
           |> Seq.toList

Player.Play tune
