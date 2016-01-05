// splitList the input list
let splitList divSize lst = 
  let rec splitAcc divSize cont = function
    | [] -> cont([],[])
    | l when divSize = 0 -> cont([], l)
    | h::t -> splitAcc (divSize-1) (fun acc -> cont(h::fst acc, snd acc)) t
  splitAcc divSize (fun x -> x) lst

// merge two sub-lists
let merge l r =
  let rec mergeCont cont l r = 
    match l, r with
    | l, [] -> cont l
    | [], r -> cont r
    | hl::tl, hr::tr ->
      if hl<hr then mergeCont (fun acc -> cont(hl::acc)) tl r
      else mergeCont (fun acc -> cont(hr::acc)) l tr
  mergeCont (fun x -> x) l r

// Sorting via merge
let mergeSort lst = 
  let rec mergeSortCont lst cont =
    match lst with
    | [] -> cont([])
    | [x] -> cont([x])
    | l -> let left, right = splitList (l.Length/2) l
           mergeSortCont left  (fun accLeft ->
           mergeSortCont right (fun accRight -> cont(merge accLeft accRight)))
  mergeSortCont lst (fun x -> x)

// initialization function
let randFunc = 
  let rnd = (new System.Random(int System.DateTime.Now.Ticks)).Next
  rnd

// create a random list
let randomList = List.init 1000000 randFunc

// result:
let res = mergeSort randomList
(*
val randomList : int list =
  [0; 0; 0; 1; 3; 0; 2; 1; 6; 4; 2; 1; 0; 7; 1; 4; 13; 4; 17; 15; 18; 7; 7; 15;
   4; 24; 20; 9; 13; 1; 13; 8; 1; 9; 25; 25; 8; 19; 36; 19; 29; 27; 25; 25; 22;
   18; 35; 17; 14; 47; 11; 16; 17; 41; 16; 8; 3; 34; 26; 54; 22; 2; 43; 51; 6;
   64; 7; 50; 53; 28; 4; 22; 22; 36; 4; 18; 56; 56; 12; 60; 16; 32; 2; 41; 68;
   11; 80; 21; 86; 53; 58; 58; 5; 2; 10; 65; 92; 76; 48; 51; ...]
val res : int list =
  [0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 1; 1; 1; 1; 1; 1; 1; 1; 1; 1; 1;
   2; 2; 2; 2; 2; 2; 2; 2; 2; 2; 2; 2; 2; 2; 2; 3; 3; 3; 3; 3; 3; 3; 3; 3; 4;
   4; 4; 4; 4; 4; 4; 4; 4; 4; 4; 4; 4; 4; 4; 5; 5; 5; 5; 5; 5; 5; 5; 5; 5; 5;
   5; 5; 6; 6; 6; 6; 6; 6; 6; 6; 7; 7; 7; 7; 7; 7; 7; 7; 7; 7; 7; 7; 7; 7; 7;
   ...]
*)