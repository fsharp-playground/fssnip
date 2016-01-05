(*****************************************************)
(* simple implementation of n-grams algorithm       **)
(* using dictionaries based on basic binary trees   **)
(*****************************************************)

open System.IO
open System.Text.RegularExpressions
let flip f x y = f y x
let curry f x y = f (x, y)
let uncurry f (x, y) = f x y

type 'a Tree when 'a : comparison = 
     | Nil 
     | Node of 'a * 'a Tree * 'a Tree

let Leaf x = Node(x, Nil, Nil)

let insert x T =
    let rec insert' x T cont = 
        match T with
        | Nil           -> cont (Leaf x)
        | Node(a, L, R) ->
          if (x > a) then
              insert' x R (fun e -> cont <| Node(a, L, e))
          else
              insert' x L (fun e -> cont <| Node(a, e, R))
    insert' x T id

let fold_infix f acc T =
    let rec fold_infix' f acc T cont = 
        match T with
        | Nil -> cont acc
        | Node(a, L, R) ->
          fold_infix' f acc L <| 
          fun e -> fold_infix' f (f e a) R cont
    fold_infix' f acc T id
        

let listToTree L = List.fold (flip insert) Nil L
let treeToList T = fold_infix (fun l x -> x::l) [] T
let treeSort L = treeToList <| listToTree L

let emptyDict = Nil
(* let insert_word word count dict = insert (count, word) dict *)

let update_word x T =
    let rec update_word' x T cont =
        match T with
        | Nil                -> cont <| Leaf (x, 1)
        | Node((w, c), L, R) ->
        if (w = x) then
            cont <| Node((w, c+1), L, R)
        elif (x < w) then
            update_word' x L (fun t -> cont <| Node((w, c), t, R))
        else
            update_word' x R (fun t -> cont <| Node((w, c), L, t))
    update_word' x T id
        
let rec lookup_word x = function 
    | Nil -> None
    | Node((w, c), L, R) ->
      if (w = x) then Some(c)
      elif (x < w) then
          lookup_word x L
      else lookup_word x R
       
(*let insert_dict word T = 
    if (None = lookup_word word T) then
        insert (word, 1) T
    else update_word word T*)

let walk_dict f = fold_infix (fun acc x -> f x) 0


(* Seq.take/Seq.truncate не работают, т.к.
требуется потом приводить их к list, 
что, наверное, медленее *)
let take n L =
    let rec take' n L cont = 
        match L with
        | []               -> cont []
        | (x::xs) when n=1 -> (cont [x])
        | (x::xs)          -> take' (n-1) xs (fun l -> cont(x::l))
    if (n > 0) 
    then take' n L id 
    else []

let ngrams len items =
    let rec ngrams' len items acc = 
        if (List.length items) < len then
            acc
        else 
            let el = take len items
            ngrams' len (List.tail items) (el::acc)
    ngrams' len items []

(* не получается записать без аргументов? *)
let bigrams W = ngrams 2 W
let trigrams W = ngrams 3 W
let monograms W = ngrams 1 W

let generateDict words dict = 
    List.fold (flip update_word) dict words


let rnd_num a b =
    let r = new System.Random()
    let n = r.Next(a, b+1)
    n

let predict word db = 
    let collect_words = fold_infix (fun acc x -> x::acc) 
    let rec lookup x T acc cont  =
        match T with
        | Nil -> cont acc
        | Node((w, c), L, R) ->
            if (x = List.head w) then
               lookup x L ((w,c)::acc) 
               <| fun e -> lookup x R (e@acc) cont
            elif (x < List.head w) then
               lookup x L acc cont
            else lookup x R acc cont
    let grams = lookup word db [] id
    let rpt a n = [for i in [1..n] do yield a]
    let flatgrams = List.collect (uncurry rpt) grams
    if List.isEmpty flatgrams then [""]
    else List.nth flatgrams ((rnd_num 1 (List.length flatgrams))-1)


let cleanup_string (s : string) = 
    let banned = ["the"; "a"; "of"] 
    (* list of the banned words *)
    let s = s.Trim().ToLower()
    let s' = Regex.Replace(s, @"[^a-z]", "")
    if List.exists (fun x -> x = s') banned
    then "" else s'

///let path = "C:\\Users\\Dan\\Documents\\Study\\FP\\test1.txt"
let path = "/home/d/Dropbox/Fp/test1.txt"
let text = File.ReadAllText(path)
let words = 
    Array.foldBack (fun e acc -> 
                    let e' = (cleanup_string e)
                    if e' = "" then acc else e'::acc)
                   (text.Split([|' ';'\n'|])) []

let dictTree = generateDict (bigrams words) Nil 
               |> generateDict (trigrams words)

let say = List.reduce (fun a x -> a + " " + x) <<
          (flip predict) dictTree 

say "cathedral"

