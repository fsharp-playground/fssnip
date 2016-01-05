let s = "this is a string"
let splits =
  s.ToCharArray()
  |> List.ofArray

let rec group groups letters =
  match letters with
  | x1::x2::x3::remainder -> [x1; x2; x3] :: group groups remainder
  | _ -> groups

group [] splits
  |> printfn "%A"

(*
$ fsharpi /tmp/test.fsx
[['t'; 'h'; 'i']; ['s'; ' '; 'i']; ['s'; ' '; 'a']; [' '; 's'; 't'];
 ['r'; 'i'; 'n']]
*)