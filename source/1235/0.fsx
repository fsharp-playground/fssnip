// like C#'s ?? operator but for options
let inline (?|) x y = match x with None -> y | _ -> x

// return first Some in a sequence
let first xs = Seq.fold (?|) None xs

// test 

None ?| Some 1 ?| Some 2 ?| None
// val it : int option = Some 1

first [None; Some 1; Some 2; None]
// val it : int option = Some 1
