(* Origionally from https://blogs.msdn.com/b/christianb/archive/2011/06/24/a-porter-stemmer-in-f.aspx
*)

type step = string -> string

type Letter = 
    Empty
  | Vowel of char
  | Conso of char

let letterToString = function
    Empty        -> ""
  | Vowel v      -> string v  
  | Conso c      -> string c
    
let isVowel v = Seq.exists ((=) v) "aeiouAEIOU"
let isVowelOnly s = s |> Seq.forall isVowel
let (|C|V|) s = if isVowel s then V s else C s

let categoriseWord w = 
 
  let idx p s = Seq.findIndex ((=) p) s
  let nth s = Seq.nth (idx s w - 1) w
  
  Seq.map(
    function  
      V s -> Vowel s 
    | s   -> 
      if idx s w = 0 then Conso s
      elif s = 'y' || s = 'Y' then 
        match nth s with
          V _ -> Conso s
        | s   -> Vowel s
      else Conso s) w

let containsVowel s = 
   categoriseWord s
   |> Seq.exists (function Vowel _ -> true | _ -> false)
   
(* Fix below 3 functions at some stage, they work just seems there is 5 times more code than necess. *)
let measureWord w = 
  if (Seq.length w < 2) then 0
  else 
    let rec measureWord' (curWord:Letter seq) (previousLetter:Letter) curScore = 
      if (Seq.isEmpty curWord) then curScore
      else
        let curLetter = Seq.head curWord
        match previousLetter, curLetter with
          Vowel _, Conso _ -> measureWord' (Seq.skip 1 curWord) curLetter (curScore + 1)
        | _, _ -> measureWord' (Seq.skip 1 curWord) curLetter curScore
    measureWord' (categoriseWord w) Empty 0

let endsWithDoubleConsonant word = 
  if word = "" then false
  else
    let categorised = categoriseWord word
    let sLen = Seq.length categorised
    let preLast = Seq.nth (sLen - 2) categorised

    match preLast with
    | Conso c -> 
        let last = Seq.nth ( sLen - 1) categorised
        match last with
        | Conso d when c = d -> true
        | _ -> false
    | _ -> false
 
let endsWithOCondition word = 
  if word = "" then false
  else
    let categorised = categoriseWord word
    let len = Seq.length categorised
    if len < 3 then false
    else
      let seqMaxIndex = len - 1
      let prePrelast = Seq.nth (seqMaxIndex - 2)  categorised
      match prePrelast with
      | Conso c ->
        let preLast = Seq.nth (seqMaxIndex - 1) categorised
        match preLast with
        | Vowel _->
          let last = Seq.nth seqMaxIndex categorised
          match last with
          | Conso d -> ( d <> 'w' && d <> 'x' && d <> 'y' )
          | _ -> false
        | _ -> false
      | _ -> false


let porterStem (word : string) =
  
  let len (s : string) = s.Length
  
  let substr (w : string) i = w.Substring(0, len w - i)

  let (|E|_|) (n,r) (h : string) = 
    if h.EndsWith n then Some(h,n,r) else None
    
  let (|LT|_|) i (w : string)  =
    if len w < i then Some()
    else None
    
  let replaceLast (w : string) (s : string) r = 
    let place = w.LastIndexOf s
    w.Remove(place, len s).Insert(place,r)
    
  let replaceStr (word,(a : string),b) k =
    if measureWord (substr word a.Length) > k
      then replaceLast word a b
    else word 
      
  let step1A : step = function
      LT 3 as w -> w
    | E ("sses", "ss") r | E ("ies" , "i" ) r
    | E ("ss"  , "ss") r | E ("s"   ,  "" ) r 
        -> replaceStr r 0
    | e -> e
          
  (* parameters everywhere *)
  let step1B word =

    let secondLast (w : string) =
      let endIfO w = 
        if endsWithOCondition w && measureWord w = 1 
          then w ^ "e" 
          else w
      
      let endIfD w = endsWithDoubleConsonant w
      match Seq.nth (len w - 1) w with
      | 'l' | 's' | 'z' -> w
      | _ -> if endIfD w then substr w 1
                         else w

    let step1Bx : step = function
        LT 4 as w -> w
      | E ("at" , "ate") r 
      | E ("bl" , "ble") r 
      | E ("iz" , "ize") r -> replaceStr r -1
      | w                  -> secondLast w

    let c w =
      let (word,_,_) = w
      let (_,s,_) = w
      if containsVowel (substr word (len s) )
        then substr word (len s) 
      else word
      
    match word with
      E ("eed" , "ee") r -> replaceStr r 0
    | E ("ed"  , "")   r -> c r |> step1Bx
    | E ("ing" , "")   r -> c r |> step1Bx
    | w -> w
    
  let step1C : step = function
      LT 4 as w -> w 
    | w when containsVowel (substr w 0) && w.EndsWith "y" -> replaceLast w "y" "i"
    | w ->   w
    
  let step2 : step = function 
      LT 4 as w -> w
    | E  ("ational", "ate" ) r | E  ("tional" , "tion") r
    | E  ("enci"   , "ence") r | E  ("anci"   , "ance") r
    | E  ("izer"   , "ize" ) r | E  ("bli"    , "ble" ) r
    | E  ("alli"   , "al"  ) r | E  ("entli"  , "ent" ) r
    | E  ("eli"    , "e"   ) r | E  ("ousli"  , "ous" ) r
    | E  ("ization", "ize" ) r | E  ("ation"  , "ate" ) r
    | E  ("ator"   , "ate" ) r | E  ("alism"  , "al"  ) r
    | E  ("iveness", "ive" ) r | E  ("fulness", "ful" ) r
    | E  ("ousness", "ous" ) r | E  ("aliti"  , "al"  ) r
    | E  ("iviti"  , "ive" ) r | E  ("biliti" , "ble" ) r
    | E  ("logi"   , "log" ) r -> replaceStr r 0
    | w                        -> w
    
  let step3 : step = function
      LT 3 as w -> w
    | E ("icate", "ic") r | E ("ative", ""  ) r
    | E ("alize", "al") r | E ("iciti", "ic") r
    | E ("ical" , "ic") r | E ("ful"  , ""  ) r
    | E ("ness" , ""  ) r -> replaceStr r 0
    | w                   -> w
        
  let step4 : step = function
      LT 3 as w -> w
    | E ("al"   , "") r | E ("ance", "") r  
    | E ("ence" , "") r | E ("er"  , "") r 
    | E ("ic"   , "") r | E ("able", "") r  
    | E ("ible" , "") r | E ("ant" , "") r  
    | E ("ement", "") r | E ("ment", "") r  
    | E ("ion"  , "") r | E ("ism" , "") r 
    | E ("ou"   , "") r | E ("iti" , "") r  
    | E ("ate"  , "") r | E ("ive" , "") r 
    | E ("ous"  , "") r | E ("ize" , "") r 
    | E ("ise"  , "") r -> replaceStr r 1
    | w                 -> w

  let step5 : step = function
      LT 2 as w -> w
    | E ("e", "e") r as w -> 
      let s = substr w 1
      let o = endsWithOCondition s
      if measureWord s > 1 || measureWord s = 1 && not o
        then replaceLast w "e" "" 
      else w
    | w -> if w.EndsWith "l" && measureWord (substr w 1) > 1 
           then substr w 1 
           else w
      
  word |> step1A |> step1B |> step1C |> step2 |> step3 |> step4 |> step5

let ps word = 
  match word with
  | "" -> None
  | w when w.Length < 2  -> Some w
  | w when isVowelOnly word -> Some w
  | w -> 
    match (w.ToLower()) with
      wl when wl.Length = 2 && wl.[1] = 'y' && isVowel wl.[0] -> Some wl
    | w -> Some (porterStem w)
    
let test = "Dumplings are cooked balls of dough. They are based on flour, potatoes or bread, and may include meat, fish, vegetables, or sweets. They may be cooked by boiling, steaming, simmering, frying, or baking. They may have a filling, or there may be other ingredients mixed into the dough. Dumplings may be sweet or spicy. They can be eaten by themselves, in soups or stews, with gravy, or in any other way. While some dumplings resemble solid water boiled doughs, such as gnocchi, others such as wontons resemble meatballs with a thin dough covering."    
test.Replace(",","").Replace(".","").Split ' ' |> Seq.map ps |> Seq.iter (fun x -> printf "%s " x.Value)
