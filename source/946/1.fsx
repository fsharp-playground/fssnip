module Seq 

/// Break a sequence into sub-sequences, where the break occurs at points
/// where the specified function returns true when provided with the n'th
/// and the n+1'th elements of the input sequence.
let BreakOn (f : 'a -> 'a -> bool) (s : seq<'a>) =
    if s |> Seq.isEmpty then
        Seq.empty
    else
        let len = s |> Seq.length
        let last = s |> Seq.nth (len-1)
        let pairs = s |> Seq.pairwise |> Seq.map (fun (x, y) -> x, Some(y))
        let pairs' = Seq.append pairs [last, None]
        seq {
            let acc = ref(Seq.empty)
            for x, y in pairs' do
                if (y.IsSome && (f x y.Value))
                   || y.IsNone then
                    yield (Seq.append !acc [x])
                    acc := Seq.empty
                else
                    acc := Seq.append !acc [x]
        }

// Examples:

let FilesByExt() = 
    System.IO.Directory.EnumerateFiles(@"d:\temp\")
    |> Seq.sortBy (fun name -> System.IO.Path.GetExtension(name.ToUpper()))
    |> BreakOn (fun name1 name2 -> System.IO.Path.GetExtension(name1.ToUpper()) <> System.IO.Path.GetExtension(name2.ToUpper()))
    |> Seq.iter (fun group -> group |> Seq.iter (fun name -> printfn "%s" name); printfn "---------------------------")
// d:\temp\expmvc.bak
// d:\temp\EXPMVC_ICI.BAK
// d:\temp\EXPMVC_ICI_BBC.BAK
// ---------------------------
// d:\temp\file.csv
// ---------------------------
// d:\temp\1m.txt.dedupe
// d:\temp\29m.txt.dedupe
// d:\temp\29mx4.txt.dedupe
// ---------------------------
// d:\temp\stainedglasslatinsquare.html
// d:\temp\window2.html
// ---------------------------

let Paginate() =
    [1..200] |> BreakOn (fun page _ -> page % 60 = 0)
// > Paginate();;
// val it : seq<seq<int>> =
//   seq
//     [seq [1; 2; 3; 4; ...]; 
//      seq [61; 62; 63; 64; ...];
//      seq [121; 122; 123; 124; ...]; 
//      seq [181; 182; 183; 184; ...]]

// See http://fssnip.net/fe
let Conway (seed : string) =
    seed
    |> BreakOn (<>)
    |> Seq.map (fun grp -> let count, character = grp |> Seq.length, grp |> Seq.nth 0
                           count.ToString() + character.ToString() )
    |> Seq.concat
    |> Seq.fold (fun acc elem -> sprintf "%s%c" acc elem) ""
// > Conway "1211";;
// val it : string = "111221"

let ConwaySeq (first : string) =
    Seq.unfold (fun seed -> Some(seed, Conway seed)) first
// > ConwaySeq "1" |> Seq.take 5 |> List.ofSeq;;
// val it : string list = ["1"; "11"; "21"; "1211"; "111221"]





