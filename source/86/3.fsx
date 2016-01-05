// the original breakBy, made more idiomatic with Rotaerk's help
let breakByV1 n s = 
    let filter k (i,x) = ((i/n) = k)
    let index = Seq.mapi (fun i x -> (i,x))
    let rec loop s = 
        seq { if not (Seq.isEmpty s) then 
                let k = (s |> Seq.head |> fst) / n
                yield (s |> Seq.truncate n
                            |> Seq.map snd)
                yield! loop (s |> Seq.skipWhile (filter k)) }
    loop (s |> index)
seq {1..25000} |> breakByV1 50
(*
val it : seq<seq<int>> =
  seq
    [seq [1; 2; 3; 4; ...]; seq [51; 52; 53; 54; ...];
     seq [101; 102; 103; 104; ...]; seq [151; 152; 153; 154; ...]; ...] *)

// with even greater Rotaerk's help, breakBy is now shorter and a couple useful
// util functions materialize
let tuple2 x y = x, y
let trim n = Seq.map snd << Seq.filter (fst >> (<=) n) << Seq.mapi tuple2
seq {1..25000} |> trim 50
//val it : seq<int> = seq [51; 52; 53; 54; ...]

let breakByV2 n s = 
    let rec loop s = 
        seq { if not (Seq.isEmpty s) then 
                yield (s |> Seq.truncate n)
                yield! loop (s |> trim n) }
    loop s
seq {1..25000} |> breakByV2 50
(*
val it : seq<seq<int>> =
  seq
    [seq [1; 2; 3; 4; ...]; seq [51; 52; 53; 54; ...];
     seq [101; 102; 103; 104; ...]; seq [151; 152; 153; 154; ...]; ...] *)

// in discussions with Rotaerk, it came out that it would be useful to return
// both first n elements and remaining sequence, in order to iterate seq in one
// pass. Rotaerk liked the name "trim" for that function, I decided on "spill".
// dgfitch helped me pinpoint the problem with spill and led me to add |> Seq.cache
// also this last version returns a sequence of lists, unavoidably I'm afraid.
// Well, I could wrap lists in seqs but that's just sugar.
let spill (n:int) (s:seq<'a>)  = 
    let en = s.GetEnumerator()
    let pos = ref 0
    let lst = [ while !pos < n && en.MoveNext() do 
                    pos := !pos+1  
                    yield en.Current]
    (lst, seq { while en.MoveNext() do yield en.Current} |> Seq.cache )
seq {1..25000} |> spill 50
(*
val it : int list * seq<int> =
  ([1; 2; 3; 4; 5; 6; 7; 8; 9; 10; 11; 12; 13; 14; 15; 16; 17; 18; 19; 20; 21;
    22; 23; 24; 25; 26; 27; 28; 29; 30; 31; 32; 33; 34; 35; 36; 37; 38; 39; 40;
    41; 42; 43; 44; 45; 46; 47; 48; 49; 50], seq [51; 52; 53; 54; ...]) *)

let breakByV3 n s = 
    s |> Seq.unfold (function 
                        | s when s |> Seq.isEmpty -> None
                        | s -> Some(s |> spill n))
seq {1..25000} |> breakByV3 50

(*
val it : seq<int list> =
  seq
    [[1; 2; 3; 4; 5; 6; 7; 8; 9; 10; 11; 12; 13; 14; 15; 16; 17; 18; 19; 20;
      21; 22; 23; 24; 25; 26; 27; 28; 29; 30; 31; 32; 33; 34; 35; 36; 37; 38;
      39; 40; 41; 42; 43; 44; 45; 46; 47; 48; 49; 50];
     [51; 52; 53; 54; 55; 56; 57; 58; 59; 60; 61; 62; 63; 64; 65; 66; 67; 68;
      69; 70; 71; 72; 73; 74; 75; 76; 77; 78; 79; 80; 81; 82; 83; 84; 85; 86;
      87; 88; 89; 90; 91; 92; 93; 94; 95; 96; 97; 98; 99; 100];
     [101; 102; 103; 104; 105; 106; 107; 108; 109; 110; 111; 112; 113; 114;
      115; 116; 117; 118; 119; 120; 121; 122; 123; 124; 125; 126; 127; 128;
      129; 130; 131; 132; 133; 134; 135; 136; 137; 138; 139; 140; 141; 142;
      143; 144; 145; 146; 147; 148; 149; 150];
     [151; 152; 153; 154; 155; 156; 157; 158; 159; 160; 161; 162; 163; 164;
      165; 166; 167; 168; 169; 170; 171; 172; 173; 174; 175; 176; 177; 178;
      179; 180; 181; 182; 183; 184; 185; 186; 187; 188; 189; 190; 191; 192;
      193; 194; 195; 196; 197; 198; 199; 200]; ...] *)

// a few timing tests demonstrating the superiority of the third version
(*
> for x in [1..2500] |> breakByV1 50 do printf"";;
Real: 00:00:00.203, CPU: 00:00:00.203, GC gen0: 1, gen1: 0, gen2: 0
val it : unit = ()
> for x in [1..2500] |> breakByV2 50 do printf"";;
Real: 00:00:00.240, CPU: 00:00:00.250, GC gen0: 6, gen1: 1, gen2: 0
val it : unit = ()
> for x in [1..2500] |> breakByV3 50 do printf"";;
Real: 00:00:00.026, CPU: 00:00:00.015, GC gen0: 1, gen1: 0, gen2: 0
val it : unit = ()
> for x in [1..10000] |> breakByV1 50 do printf"";;
Real: 00:00:10.746, CPU: 00:00:10.734, GC gen0: 10, gen1: 1, gen2: 0
val it : unit = ()
> for x in [1..10000] |> breakByV2 50 do printf"";;
Real: 00:00:13.781, CPU: 00:00:13.921, GC gen0: 345, gen1: 4, gen2: 1
val it : unit = ()
> for x in [1..10000] |> breakByV3 50 do printf"";;
Real: 00:00:00.380, CPU: 00:00:00.375, GC gen0: 6, gen1: 2, gen2: 0
val it : unit = ()
> for x in [1..10000] |> breakByV1 500 do printf"";;
Real: 00:00:00.139, CPU: 00:00:00.156, GC gen0: 1, gen1: 0, gen2: 0
val it : unit = ()
> for x in [1..10000] |> breakByV2 500 do printf"";;
Real: 00:00:00.165, CPU: 00:00:00.171, GC gen0: 4, gen1: 0, gen2: 0
val it : unit = ()
> for x in [1..10000] |> breakByV3 500 do printf"";;
Real: 00:00:00.038, CPU: 00:00:00.046, GC gen0: 1, gen1: 0, gen2: 0
val it : unit = ()
> for x in [1..100000] |> breakByV1 500 do printf"";;
Real: 00:01:40.741, CPU: 00:01:40.812, GC gen0: 103, gen1: 10, gen2: 1
val it : unit = ()
> for x in [1..100000] |> breakByV2 500 do printf"";;
Real: 00:02:22.935, CPU: 00:02:22.968, GC gen0: 3444, gen1: 18, gen2: 2
val it : unit = ()
> for x in [1..100000] |> breakByV3 500 do printf"";;
Real: 00:00:04.255, CPU: 00:00:04.453, GC gen0: 61, gen1: 14, gen2: 2
val it : unit = ()
> *)