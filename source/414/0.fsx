open System
open System.Linq
open System.Text

module Soundex =
    /// The Soundex code for a name consists of a letter followed by three numerical digits: the letter 
    /// is the first letter of the name, and the digits encode the remaining consonants. 
    /// The National Archives and Records Administration (NARA) maintains the current rule set for the 
    /// official implementation of Soundex used by the U.S. Government. The American Soundex is a variant 
    /// of the original Russell Soundex algorithm.
    /// Reference: http://www.archives.gov/research/census/soundex.html
    let American(text : string) =
        let chooser c = 
            match Char.ToLowerInvariant(c) with
            | 'b' | 'f' | 'p' | 'v' -> '1'
            | 'c' | 'g' | 'j' | 'k' | 'q' | 's' | 'x' | 'z' -> '2'
            | 'd' | 't' -> '3'
            | 'l' -> '4'
            | 'm' | 'n' -> '5'
            | 'r' -> '6'
            | 'h' | 'w' -> '-'                        
            | _ -> if Char.IsDigit(c) then c else '.'

        let folder (state : char list) (c) =
            match chooser(Seq.head state), chooser(c) with
            | p, i when (p <> i && i <> '-') || i = '0' -> i :: state 
            | _, _ -> state                    

        let value = text.Trim() + "000"
        let soundex = Seq.toList (value.Substring(1))
                      |> List.fold folder [ Char.ToUpperInvariant(value.[0]) ]
                      |> List.filter (fun c -> c <> '.')
                      |> List.rev
                      |> Seq.truncate 4
                      |> Seq.toArray        
        String(soundex)

    /// Daitch–Mokotoff Soundex (D–M Soundex) was developed in 1985 by genealogist Gary Mokotoff and later 
    /// improved by genealogist Randy Daitch because of problems they encountered while trying to apply the 
    /// Russell Soundex to Jews with Germanic or Slavic surnames.
    /// References:
    /// <list type="bullet">
    /// <description>http://www.avotaynu.com/soundex.htm</description>
    /// <description>http://www.jewishgen.org/InfoFiles/soundex.html</description>
    /// </list>
    let DaitchMokotoff(text : string) =

        let isVowel(value : string) =
            let first = if String.IsNullOrEmpty(value) then String.Empty else value.Substring(0, 1)
            match first with
            | "A" | "E" | "I" | "O" | "U" | "Y" -> true
            | _ -> false
        
        let (|Match|_|) (values : seq<string>) (codes : seq<string>) (s : string) = 
            let result = Seq.tryFind (fun value -> s.StartsWith(value)) values
            if result.IsNone then None else Some( codes, isVowel(result.Value), s.Substring(result.Value.Length)) 
            
        let (|Group|_|) (s : string) = 
            match s with
            | Match ["AI";"AJ";"AY"]        ["0"; "1"; ""]      result -> Some(result)
            | Match ["AU"]                  ["0"; "7"; ""]      result -> Some(result)
            | Match ["A"]                   ["0"; ""; ""]       result -> Some(result)
            | Match ["B"]                   ["7"; "7"; "7"]     result -> Some(result)
            | Match ["CHS"]                 ["5"; "54"; "54"]   result -> Some(result)
            | Match ["CH"]                  ["KH"; "TCH"]       result -> Some(result)
            | Match ["CK"]                  ["K"; "TSK"]        result -> Some(result)
            | Match ["CZS";"CZ";"CSZ";"CS"] ["4"; "4"; "4"]     result -> Some(result)
            | Match ["C"]                   ["K"; "TZ"]         result -> Some(result)
            | Match ["DRZ";"DRS"]           ["4"; "4"; "4"]     result -> Some(result)
            | Match ["DSZ";"DSH";"DS"]      ["4"; "4"; "4"]     result -> Some(result)
            | Match ["DZH";"DZS";"DZ"]      ["4"; "4"; "4"]     result -> Some(result)
            | Match ["DT"]                  ["3"; "3"; "3"]     result -> Some(result)
            | Match ["EI";"EJ";"EY"]        ["0"; "1"; ""]      result -> Some(result)
            | Match ["EU"]                  ["1"; "1"; ""]      result -> Some(result)
            | Match ["E"]                   ["0"; ""; ""]       result -> Some(result)
            | Match ["FB"; "F"]             ["7"; "7"; "7"]     result -> Some(result)
            | Match ["G"]                   ["5"; "5"; "5"]     result -> Some(result)
            | Match ["H"]                   ["5"; "5"; ""]      result -> Some(result)
            | Match ["IA";"IE";"IO";"IU"]   ["1"; ""; ""]       result -> Some(result)
            | Match ["I"]                   ["0"; ""; ""]       result -> Some(result)
            | Match ["J2Y"]                 ["1"; "1"; "1"]     result -> Some(result)
            | Match ["J"]                   ["J2Y"; "DZH"]      result -> Some(result)
            | Match ["KS"]                  ["5"; "54"; "54"]   result -> Some(result)
            | Match ["KH"; "K"]             ["5"; "5"; "5"]     result -> Some(result)
            | Match ["L"]                   ["8"; "8"; "8"]     result -> Some(result)
            | Match ["MN";"NM"]             ["66"; "66"; "66"]  result -> Some(result)
            | Match ["M"; "N"]              ["6"; "6"; "6"]     result -> Some(result)
            | Match ["OI";"OJ";"OY"]        ["0"; "1"; ""]      result -> Some(result)
            | Match ["O"]                   ["0"; ""; ""]       result -> Some(result)
            | Match ["PF";"PH"; "P"]        ["7"; "7"; "7"]     result -> Some(result)
            | Match ["RTZ"]                 ["94"; "94"; "94"]  result -> Some(result)
            | Match ["RS";"RZ"]             ["RTZ"; "ZH"]       result -> Some(result)
            | Match ["R"]                   ["9"; "9"; "9"]     result -> Some(result)
            | Match ["SCHTSCH";"SCHTSH";"SCHTCH"] ["2"; "4"; "4"] result -> Some(result)
            | Match ["SCH"]                 ["4"; "4"; "4"]     result -> Some(result)
            | Match ["SHTCH";"SHCH";"SHTSH"] ["2"; "4"; "4"]    result -> Some(result)
            | Match ["SHT";"SCHT";"SCHD"]   ["2"; "43"; "43"]   result -> Some(result)
            | Match ["SH"]                  ["4"; "4"; "4"]     result -> Some(result)
            | Match ["STCH";"STSCH";"SC"]   ["2"; "4"; "4"]     result -> Some(result)
            | Match ["STRZ";"STRS";"STSH"]  ["2"; "4"; "4"]     result -> Some(result)
            | Match ["ST"]                  ["2"; "43"; "43"]   result -> Some(result)
            | Match ["SZCZ";"SZCS"]         ["2"; "4"; "4"]     result -> Some(result)
            | Match ["SZT";"SHD";"SZD";"SD"] ["2"; "43"; "43"]  result -> Some(result)
            | Match ["SZ";"S"]              ["4"; "4"; "4"]     result -> Some(result)
            | Match ["TCH";"TTCH";"TTSCH";"THS"] ["4";"4";"4"]  result -> Some(result)
            | Match ["TH"]                  ["3";"3";"3"]       result -> Some(result)
            | Match ["TRZ";"TRS"]           ["4";"4";"4"]       result -> Some(result)
            | Match ["TSCH";"TSH"]          ["4";"4";"4"]       result -> Some(result)
            | Match ["TSK"]                 ["45";"45";"45"]    result -> Some(result)
            | Match ["TTSZ";"TTS";"TC"]     ["4";"4";"4"]       result -> Some(result)
            | Match ["TZS";"TTZ";"TZ";"TSZ";"TS"] ["4";"4";"4"] result -> Some(result)
            | Match ["T"]                   ["3";"3";"3"]       result -> Some(result)
            | Match ["UI";"UJ";"UY"]        ["0";"1";""]        result -> Some(result)
            | Match ["UE";"U"]              ["0";"";""]         result -> Some(result)
            | Match ["V"]                   ["7";"7";"7"]       result -> Some(result)
            | Match ["W"]                   ["7";"7";"7"]       result -> Some(result)
            | Match ["X"]                   ["5";"54";"54"]     result -> Some(result)
            | Match ["Y"]                   ["1";"";""]         result -> Some(result)
            | Match ["ZHDZH";"ZDZH";"ZDZ"]  ["2";"4";"4"]       result -> Some(result)
            | Match ["ZD";"ZHD"]            ["2";"43";"43"]     result -> Some(result)
            | Match ["ZSCH";"ZSH";"ZH";"ZS"] ["4";"4";"4"]      result -> Some(result)
            | Match ["Z"]                   ["4";"4";"4"]       result -> Some(result)
            | _ -> None

        let search(value : string) =
            match value with            
            | Group result -> (*printfn "%s -> %A" value result;*) result
            | _ -> seq[ "" ], false, value.Substring(1)

        let rec decompose (value : string) = 
            let head, isVowel, next = search(value)
            if String.IsNullOrEmpty(next) then 
                seq [ head, isVowel ] 
            else 
                Seq.append [ head, isVowel ] (decompose(next))
        
        let rec encode (codes : seq<string>, start : bool, beforeVowel : bool) =
            let first = Seq.nth 0 codes
            if String.IsNullOrEmpty(first) then
                [ ]
            else
                if String.IsNullOrEmpty(first) || Char.IsDigit(Seq.nth 0 first) then
                    if start then
                        [ (Seq.nth 0 codes) ]
                    else if beforeVowel then
                        [ (Seq.nth 1 codes) ]
                    else
                        [ (Seq.nth 2 codes) ]
                else            
                    let head, isVowel, next = search(first)
                    if Seq.length codes = 1 then 
                        encode(head, start, beforeVowel) 
                    else 
                        List.append (encode(head, start, beforeVowel)) (encode(Seq.skip 1 codes, start, beforeVowel))
                
        let reduce (results : string list) (values : string list) = 
            let appender(value : string) = 
                fun (s : string) -> 
                    let result = (s + value)
                    if result.Length >= 6 then result.Substring(0, 6) else result
            seq { for value in values do yield List.map (appender(value)) results }
            |> Seq.concat
            |> Seq.toList

        let first, second = decompose ( text.ToUpperInvariant() ) 
                            |> Seq.pairwise
                            |> Seq.filter (fun (a, b) -> a <> b)
                            |> Seq.toList
                            |> List.unzip                                    
        let items, vowels = List.unzip (List.head first :: second)

        List.zip items ( List.append (List.tail vowels) [ false ] )
        |> List.mapi (fun (i) (codes, isVowel) -> encode(codes, i = 0, isVowel))
        |> List.filter (fun c -> not(List.isEmpty c) && (List.head c) <> "")
        |> List.reduce reduce 
        |> List.toSeq
        |> Seq.distinct
        |> Seq.map (fun s -> if s.Length >= 6 then s else String.Concat(s, String('0', 6 - s.Length)))

(*
Soundex.American("Ashcraft") // "A261"
Soundex.American("jackson")  // "J250"
Soundex.American("miller")	 // "M460"
Soundex.American("Wilson")	 // "W425"
Soundex.American("Schmit") 	 // "S530"
Soundex.American("Lloyd")	 // "L300"

Soundex.DaitchMokotoff("Peters")	 // [ "739400"; "734000" ];
Soundex.DaitchMokotoff("Peterson")	 // [ "739460"; "734600" ];
Soundex.DaitchMokotoff("Moskowitz")	 // [ "645740" ];
Soundex.DaitchMokotoff("Moskovitz")	 // [ "645740" ];
Soundex.DaitchMokotoff("Auerbach")	 // [ "097500"; "097400" ];
Soundex.DaitchMokotoff("Uhrbach")	 // [ "097500"; "097400" ];
Soundex.DaitchMokotoff("Jackson")	 // [ "154600"; "454600"; "145460"; "445460" ];
*)