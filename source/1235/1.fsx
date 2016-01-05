// like C#'s ?? operator but for options
let inline (?|) x y = match x with None -> y | _ -> x

// test 

None ?| Some 1 ?| Some 2 ?| None 
// val it : int option = Some 1

Seq.reduce (?|) [None; Some 1; Some 2; None]
// val it : int option = Some 1