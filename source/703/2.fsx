(* Origionally from https://blogs.msdn.com/b/christianb/archive/2011/06/24/a-porter-stemmer-in-f.aspx*)
open System

type Letter = 
    Empty
  | Vowel of char
  | Conso of char
    
let isVowel v             = Seq.exists ((=) v) "aeiouAEIOU"
let isVowelOnly s         = Seq.forall isVowel s
let isConsonant c         = not (isVowel c)

let length (s : string)   = s.Length
let containsVowel s       = Seq.exists isVowel s
let substr (w : string) i = w.Substring(0, length w - i)

let categoriseWord w = 

  let (|C|V|) s = if isVowel s then V s else C s

  let idx p = Seq.findIndex ((=) p)
  let nth s = Seq.nth (idx s w - 1) w

  let cat s =
    if isVowel s then Vowel s 
    else
      if idx s w = 0 then Conso s
      elif s = 'y' || s = 'Y' then 
        match nth s with
          V _ -> Conso s
        | s   -> Vowel s
      else Conso s

  w |> Seq.map cat 

(* Fix below 3 functions at some stage, they work just seems there is 5 times more code than necess. *)
let measureWord w = 
  if Seq.length w < 2 then 0
  else 
    let rec measureWord' (curWord:Letter seq) (previousLetter:Letter) curScore = 
      if (Seq.isEmpty curWord) then curScore
      else
        let curLetter  = Seq.head curWord
        let skipLetter = Seq.skip 1 curWord
        match previousLetter, curLetter with
          Vowel _, Conso _ -> measureWord' skipLetter curLetter (curScore + 1)
        | _, _ -> measureWord' skipLetter curLetter curScore
    measureWord' (categoriseWord w) Empty 0

let endsWithDoubleConsonant (word : string) = 
  let length = length word
  let first,second = word.[length - 2], word.[length - 1]

  length > 2 && isConsonant first && first = second

let endsWithOCondition (word : string) = 
  let length = length word
  let lastLength = length - 1

  if length < 3 then false else
      
    isConsonant word.[lastLength]     &&
    isVowel     word.[lastLength - 1] &&
    isConsonant word.[lastLength - 2] &&
    Seq.exists ((<>) word.[lastLength]) "wxy"

let step1A_subs = 
  [ "sses", "ss"; "ies" , "i"
    "ss"  , "ss"; "s"   , "" ] 

let step1B_subs =
  [ "at", "ate"; "bl","ble"
    "iz", "ize"]

let step1B2_subs = 
  [ "eed", "ee" ]
  
let step2_subs =
  [ "ational", "ate" ;  "tional" , "tion" 
    "enci"   , "ence";  "anci"   , "ance" 
    "izer"   , "ize" ;  "bli"    , "ble"  
    "alli"   , "al"  ;  "entli"  , "ent"  
    "eli"    , "e"   ;  "ousli"  , "ous"  
    "ization", "ize" ;  "ation"  , "ate"  
    "ator"   , "ate" ;  "alism"  , "al"   
    "iveness", "ive" ;  "fulness", "ful"  
    "ousness", "ous" ;  "aliti"  , "al"  
    "iviti"  , "ive" ;  "biliti" , "ble" 
    "logi"   , "log" ]

let step3_subs = 
  [ "icate", "ic"; "ative", ""  
    "alize", "al"; "iciti", "ic"
    "ical" , "ic"; "ful"  , "" 
    "ness" , "" ]

let step4_subs =
  [ "al"   , ""; "ance", "" 
    "ence" , ""; "er"  , ""
    "ic"   , ""; "able", "" 
    "ible" , ""; "ant" , ""
    "ement", ""; "ment", "" 
    "ion"  , ""; "ism" , ""
    "ou"   , ""; "iti" , "" 
    "ate"  , ""; "ive" , ""
    "ous"  , ""; "ize" , ""
    "ise"  , "" ]

let stem (winput : string) =

  let replaceLast (w : string) (s : string) r = 
    let place = w.LastIndexOf s in
      if place >= 0 then 
        w.Remove(place, length s).Insert(place,r)
      else w

  let findFirstReplace (word : string) n pList =
    let cand = 
      pList 
      |> Seq.tryFind (fun (e,b) -> word.EndsWith e)
    if cand <> None then
       cand |> Option.get 
            |> fun (e,r) -> 
               if measureWord (substr word e.Length) > n
                 then replaceLast word e r
               else word
    else word

  let step1A (word : string) =
    findFirstReplace word 0 step1A_subs

  let step1B (word : string) =

    let step1Bx (w : string) =
      findFirstReplace w -1 step1B_subs

    let sub w n =
      if containsVowel (substr w n)
      then substr w n else w

    match word with
    | w when w.EndsWith "eed" -> findFirstReplace word 0 step1B2_subs
    | w when w.EndsWith "ed"  -> sub word 2
    | w when w.EndsWith "ing" -> sub word 3
    | w ->   w
  
  let step1C (w : string) =
    if containsVowel (substr w 0) && w.EndsWith "y" then
      replaceLast w "y" "i"
    else w

  let step2 (word : string) =
    findFirstReplace word 0 step2_subs

  let step3 (word : string) = 
    findFirstReplace word 0 step3_subs

  let step4 (word : string) =
    findFirstReplace word 1 step4_subs

  let step5 (word : string) = 

    if word.EndsWith "e" then
      let s = substr word 1
      if word.EndsWith "l" && measureWord s > 1 
        then s
      else
        let o = endsWithOCondition s
        if measureWord s > 1 || measureWord s = 1 && not o
          then replaceLast word "e" ""
        else word
    else word

  let c word len' f = 
    if length word < len' then word 
    else 
      let ret = f word
      if length ret > 0 then ret else word
  
  let rec step w n = 
    let s b f = step (c w b f) (n+1)
    if n < 7 then
      match n with
      | 0 -> s 3 step1A 
      | 1 -> s 0 step1B
      | 2 -> s 4 step1C 
      | 3 -> s 4 step2 
      | 4 -> s 3 step3
      | 5 -> s 3 step4 
      | 6 -> s 2 step5
      | _ -> ""
    else w
  step winput 0

let ps (winput : string) =
  if length winput < 2 || isVowelOnly winput then 
    Some winput
  else 
    let wl = winput.ToLower()
    if wl.[1] = 'y' && isVowel wl.[0] then 
      Some wl
    else Some (stem wl)

let filter (str : string) = 
  str |> Seq.filter (fun c -> Char.IsLetter c || c = ' ')
      |> Seq.map string 
      |> String.concat ""

let test = 
  "Do you really think it is weakness that yields to temptation? \
  I tell you that there are terrible temptations which it requires strength, \ 
  strength and courage to yield to"

(test |> filter).Split ' '
|> Array.filter ((<>) "")
|> Array.map ps 
|> Array.iter (fun x -> printf "%s " x.Value)

//"do you realli think it is weak that yield to temptat I tell you that there ar terribl temptat which it requir strength strength and courag to yield to"
// test below taken from http://qaa.ath.cx/porter_js_demo.html, for closer accuracy we should strip non a-z

//"Do you realli think it is weak that yield to temptat I tell you that there ar terribl temptat which it requir strength strength and courag to yield to"
