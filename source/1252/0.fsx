module Tests

open Xunit
open Xunit.Extensions
open System
open System.Reflection
open Microsoft.FSharp.Reflection

type Farbe =
    | Rot | Grün | Blau | Gelb | Violett | Schwarz | Weiß
    override self.ToString() = sprintf "%A" self

let (|Farben|_|) (farbe1: Farbe, farbe2: Farbe) =
    function
    | f1, f2 when f1 = farbe1 && f2 = farbe2 -> Some()
    | f1, f2 when f2 = farbe1 && f1 = farbe2 -> Some()
    | _ -> None

let (|Schwarz|_|) =
    function
    | Schwarz, _ -> Some()
    | _, Schwarz -> Some()
    | _ -> None

let (|Weiß|_|) =
    function
    | Weiß, farbe -> Some(farbe)
    | farbe, Weiß -> Some(farbe)
    | _ -> None

let (|GleicheFarbe|_|) =
    function
    | farbe1, farbe2 when farbe1 = farbe2 -> Some(farbe1)
    | _ -> None

let mischeFarben =
    function
    | Schwarz -> Schwarz
    | Weiß(farbe) -> farbe
    | GleicheFarbe(farbe) -> farbe
    | Farben(Blau, Rot)  -> Violett
    | Farben(Gelb, Blau) -> Grün
    | farbe1, farbe2                      -> failwithf "Unbekannte Mischung: %A + %A" farbe1 farbe2

/// DSL zum Mischen von zwei Farben
/// Beispiel: Rot <+> Blau = Violett
let (<+>) farbe1 farbe2 = mischeFarben (farbe1, farbe2)

/// Helfer-Funktion, um einen array<'a*'b> in ein array<array<obj>> (oder obj[][]) umzuwandeln
let inline toTheoryData(array: array<'a * 'b>): obj[][] =
    Array.map (fun (a, b) -> [| a; b |]) array
    
let farbmischungen =
    // Mischt man die beiden Ausgangsfarben ergibt das folgende neue Farbe
    toTheoryData [| (Rot, Blau),     Violett
                    (Blau, Rot),     Violett
                    (Blau, Gelb),    Grün
                    (Blau, Blau),    Blau
                    (Grün, Grün),    Grün
                    (Schwarz, Gelb), Schwarz
                    (Gelb, Schwarz), Schwarz
                    (Weiß, Gelb),    Gelb
                    (Gelb, Weiß),    Gelb |]

[<Theory; PropertyData("farbmischungen")>]
let ``Teste Farbmischungen``(farben, sollFarbe) =
    Assert.Equal(mischeFarben farben, sollFarbe)

let farbmischungenMit3Farben =
    toTheoryData [| (Gelb, Weiß, Schwarz), Schwarz
                    (Blau, Gelb, Weiß),    Grün |]

[<Theory; PropertyData("farbmischungenMit3Farben")>]
let ``Teste Farbmischungen mit 3 Farben``((farbe1, farbe2, farbe3), sollFarbe) =
    Assert.Equal(farbe1 <+> farbe2 <+> farbe3, sollFarbe)
