module Stemmer
open System

let (|BaseVowel|_|) a = match Char.ToLower(a) with 'a' | 'e' | 'i' | 'o' | 'u' -> Some a | _ -> None

let rec (|Consonant|_|) xs =
    match xs with
    | [] -> None
    | a::_ when Char.IsWhiteSpace(a) -> None
    | BaseVowel a::_ -> None
    | 'y'::Consonant _ | 'Y'::Consonant _-> None
    | x::rest -> Some(x,rest)
and (|Vowel|_|) xs = 
    match xs with
    | [] -> None 
    | a::_ when Char.IsWhiteSpace(a) -> None
    | Consonant _ -> None 
    | x::rest -> Some(x,rest)

let (|C|_|) xs = 
    let rec loop xs acc =
        match xs with
        | Consonant (x,xs) -> loop xs (x::acc) 
        | _ -> match acc with [] -> None | _ -> Some(acc|>List.rev,xs)
    loop xs []

let (|V|_|) xs  = 
    let rec loop xs acc =
        match xs with
        | Vowel (x,xs) -> loop xs (x::acc) 
        | _ -> match acc with [] -> None | _ -> Some(acc|>List.rev,xs)
    loop xs []

let (|VC|_|) xs  =
    let rec loop xs acc =
        match xs with 
        | [] -> match acc with [] -> None | _ -> Some(acc,xs)
        |  a::_ when Char.IsWhiteSpace(a) -> match acc with [] -> None | _ -> Some(acc,xs)
        | C (cs, V (vs,xs)) -> loop xs ((vs,cs)::acc)
        | C (cs, xs) -> loop xs (([],cs)::acc)
        | V (vs, xs) -> loop xs ((vs,[])::acc)
        | _ -> None
    loop xs []

let calcMeasure vs = (0,vs) ||> List.fold (fun c (vs,cs) -> match vs,cs with [],_ | _,[] -> c | _ -> c+1)

let (|Ends|_|) stem xs  = 
    let rec loop xs ys =
        match xs,ys with
        | [],[] -> Some(xs)
        | [],_ -> None
        | _, [] -> Some(xs)
        | a::xs,b::ys when a = b || Char.ToLower(a) = Char.ToLower(b) -> loop xs ys
        | _ -> None
    loop xs stem

let ContainsVowel = List.exists ((|BaseVowel|_|) >> Option.isSome)

let (|EndsWithDoubleC|_|) xs =
    match xs with
    | Consonant (a, Consonant (b,xs)) when a = b -> Some (a,xs)
    | _ -> None

let (|EndsWithCVC|_|) xs =
    match xs with
    | Consonant (a, Vowel (_, Consonant (_,xs))) ->
        match a with 
        | 'w' | 'x' | 'y' | 'W' | 'X' | 'Y' -> None
        | _ -> Some(xs)
    | _ -> None
let (|NotEndsWithCVC|_|) xs = match xs with EndsWithCVC (_) -> None | _ -> Some(xs)

let (|Measure|_|) xs = match xs with VC (cvs,rest) -> calcMeasure cvs |> Some | _ -> None

let Measure xs = match xs with Measure (m) -> m | _  -> 0

let step1a xs =
    match xs with
    | Ends ['s';'e';'s';'s'] (rest) -> 's'::'s'::rest
    | Ends ['s';'e';'i'] (rest) -> 'i'::rest
    | 's'::'s'::rest -> xs
    | 's'::rest -> rest
    | _ -> xs

let contains y xs  = xs |> List.exists (fun x -> x=y)

let step1bTx xs =
    match xs with
    | Ends ['t';'a'] _ -> 'e'::xs
    | Ends ['l'; 'b'] _ -> 'e'::xs
    | Ends ['z'; 'i'] _ -> 'e'::xs
    | EndsWithDoubleC (x,rest) when ['l';'s';'z'] |> contains x |> not -> x::rest
    | EndsWithCVC (_) & Measure (m) when m = 1 -> 'e'::xs
    | _ -> xs

let step1b xs =
    match xs with
    | Ends ['d';'e';'e'] rest -> if Measure rest > 0 then 'e'::'e':: rest else xs
    | Ends ['d';'e'] rest when ContainsVowel rest -> rest |> step1bTx 
    | Ends ['g';'n';'i'] rest when ContainsVowel rest -> rest |> step1bTx 
    | _ -> xs

let step1c xs = 
    match xs with 
    | Ends ['y'] rest when ContainsVowel rest -> 'i'::rest
    | _ -> xs 

let step2 xs =
    match xs with
    | Ends ['l';'a';'n';'o';'i';'t';'a'] rest when Measure rest > 0 -> 'e'::'t'::'a'::rest 
    | Ends ['l';'a';'n';'o';'i';'t'] rest when Measure rest > 0 -> 'n'::'o'::'i'::'t'::rest 
    | Ends ['i';'c';'n';'e'] rest when Measure rest > 0 -> 'e'::'c'::'n'::'e'::rest 
    | Ends ['i';'c';'n';'a'] rest when Measure rest > 0 -> 'e'::'c'::'n'::'a'::rest 
    | Ends ['r';'e';'z';'i'] rest when Measure rest > 0 -> 'e'::'z'::'i'::rest 
    | Ends ['i';'l';'b'] rest when Measure rest > 0 -> 'e'::'l'::'b'::rest 
    | Ends ['i';'l';'l';'a'] rest when Measure rest > 0 -> 'l'::'a'::rest 
    | Ends ['i';'l';'t';'n';'e'] rest when Measure rest > 0 -> 't'::'n'::'e'::rest 
    | Ends ['i';'l';'e'] rest when Measure rest > 0 -> 'e'::rest 
    | Ends ['i';'l';'s';'u';'o'] rest when Measure rest > 0 -> 's'::'u'::'o'::rest 
    | Ends ['n';'o';'i';'t';'a';'z';'i'] rest when Measure rest > 0 -> 'e'::'z'::'i'::rest 
    | Ends ['n';'o';'i';'t';'a'] rest when Measure rest > 0 -> 'e'::'t'::'a'::rest 
    | Ends ['r';'o';'t';'a'] rest when Measure rest > 0 -> 'e'::'t'::'a'::rest 
    | Ends ['m';'z';'i';'l';'a'] rest when Measure rest > 0 -> 'l'::'a'::rest 
    | Ends ['s';'s';'e';'n';'e';'v';'i'] rest when Measure rest > 0 -> 'e'::'v'::'i'::rest 
    | Ends ['s';'s';'e';'n';'l';'u';'f'] rest when Measure rest > 0 -> 'l'::'u'::'f'::rest 
    | Ends ['s';'s';'e';'n';'s';'u';'o'] rest when Measure rest > 0 -> 's'::'u'::'o'::rest 
    | Ends ['i';'t';'i';'l';'a'] rest when Measure rest > 0 -> 'l'::'a'::rest 
    | Ends ['i';'t';'i';'v';'i'] rest when Measure rest > 0 -> 'e'::'v'::'i'::rest 
    | Ends ['i';'t';'i';'l';'i';'b'] rest when Measure rest > 0 -> 'e'::'l'::'b'::rest 
    | Ends ['i';'g';'o';'l'] rest when Measure rest > 0 -> 'g'::'o'::'l'::rest 
    | _ -> xs

let step3 xs =
    match xs with
    | Ends ['e';'t';'a';'c';'i'] rest when Measure rest > 0 -> 'c'::'i'::rest 
    | Ends ['e';'v';'i';'t';'a'] rest when Measure rest > 0 -> rest 
    | Ends ['e';'z';'i';'l';'a'] rest when Measure rest > 0 -> 'l'::'a'::rest 
    | Ends ['i';'t';'i';'c';'i'] rest when Measure rest > 0 -> 'c'::'i'::rest 
    | Ends ['l';'a';'c';'i'] rest when Measure rest > 0 -> 'c'::'i'::rest 
    | Ends ['l';'u';'f'] rest when Measure rest > 0 -> rest 
    | Ends ['s';'s';'e';'n'] rest when Measure rest > 0 -> rest 
    | _ -> xs

let step4 xs =
    match xs with
    | Ends ['l';'a'] rest when Measure rest > 1 -> rest 
    | Ends ['e';'c';'n';'a'] rest when Measure rest > 1 -> rest 
    | Ends ['e';'c';'n';'e'] rest when Measure rest > 1 -> rest 
    | Ends ['r';'e'] rest when Measure rest > 1 -> rest 
    | Ends ['c';'i'] rest when Measure rest > 1 -> rest 
    | Ends ['e';'l';'b';'a'] rest when Measure rest > 1 -> rest 
    | Ends ['e';'l';'b';'i'] rest when Measure rest > 1 -> rest 
    | Ends ['t';'n';'a'] rest when Measure rest > 1 -> rest 
    | Ends ['t';'n';'e';'m';'e'] rest when Measure rest > 1 -> rest 
    | Ends ['t';'n';'e';'m'] rest when Measure rest > 1 -> rest 
    | Ends ['t';'n';'e'] rest when Measure rest > 1 -> rest 
    | Ends ['n';'o';'i'] rest when 
        match rest with 
        | Measure (m) & (Ends ['s'] (_) | Ends ['t'] (_)) when m > 1 -> true
        | _ -> false
        -> rest 
    | Ends ['u';'o'] rest when Measure rest > 1 -> rest 
    | Ends ['m';'s';'i'] rest when Measure rest > 1 -> rest 
    | Ends ['e';'t';'a'] rest when Measure rest > 1 -> rest 
    | Ends ['i';'t';'i'] rest when Measure rest > 1 -> rest 
    | Ends ['s';'u';'o'] rest when Measure rest > 1 -> rest 
    | Ends ['e';'v';'i'] rest when Measure rest > 1 -> rest 
    | Ends ['e';'z';'i'] rest when Measure rest > 1 -> rest 
    | _ -> xs

let step5a xs =
    match xs with
    | Ends ['e'] rest when Measure rest > 1 -> rest
    | Ends ['e'] rest when 
        match rest with
        | NotEndsWithCVC (_) & Measure (m) when m = 1 -> true
        | _ -> false
        -> rest
    | _ -> xs

let step5b xs =
    match xs with
    | EndsWithDoubleC ('l',rest) & Measure (m) when m > 1 -> 'l'::rest
    | _ -> xs

let stemCsRev = step1a >> step1b >> step1c >> step2 >> step3 >> step4 >> step5a >> step5b
let stemCs = List.rev >> stemCsRev >> List.rev
let stem (s:string) = System.String(s.ToLower().ToCharArray() |> Array.toList |> stemCs |> List.toArray)

(* TEST SCRIPT *)
(*
#load "Stemmer.fs"
open Stemmer

let test a b = if  a |> stem  = b then () else failwithf "%s %s" a b

//verified against NLTK implementation http://text-processing.com/demo/stem/
let testWords =
    [
        "caresses","caress"
        "ponies","poni"
        "ties","ti"
        "caress","caress"
        "cats","cat"
        "feed","feed"
        "agreed","agre"
        "plastered","plaster"
        "bled","bled"
        "motoring","motor"
        "sing","sing"
        "conflated","conflat" //"conflate"
        "troubled","troubl"
        "sized","size"
        "hopping","hop"
        "tanned","tan"
        "falling","fall"
        "hissing","hiss"
        "fizzed","fizz"
        "failing","fail"
        "filing","file"
        "relational","relat"
        "conditional","condit"
        "rational","ration"
        "valenci","valenc"
        "hesitanci","hesit"
        "digitizer","digit"
        "conformabli","conform"
        "radicalli","radic"
        "differentli","differ"
        "vileli","vile"
        "analogousli","analog"
        "vietnamizati","vietnamizati"
        "predication","predic"
        "operator","oper"
        "feudalism","feudal"
        "decisiveness","decis"
        "hopefulness","hope"
        "callousness","callous"
        "formaliti","formal"
        "sensitiviti","sensit"
        "sensibiliti","sensibl"
        "triplicate","triplic"
        "formative","form"
        "formalize","formal"
        "electriciti","electr"
        "electrical","electr"
        "hopeful","hope"
        "goodness","good"
        "revival","reviv"
        "allowance","allow"
        "inference","infer"
        "airliner","airlin"
        "gyroscopic","gyroscop"
        "adjustable","adjust"
        "defensible","defens"
        "irritant","irrit"
        "replacement","replac"
        "adjustment","adjust"
        "dependent","depend"
        "adoption","adopt"
        "homologou","homolog"
        "communism","commun"
        "activate","activ"
        "angulariti","angular"
        "homologous","homolog"
        "effective","effect"
        "bowdlerize","bowdler"
        "probate","probat"
        "rate","rate"
        "cease","ceas"
        "controll","control"
        "roll","roll"
    ]

let runTest() = 
    testWords |> List.iter (fun (a,b) -> test a b; printfn "%s -> %s" a b)
    printfn "done"
*)