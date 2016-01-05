// [snippet:Active pattern]
/// Checks to see if a list has exactly one element
/// and checks to see if that one element is a
/// particular given element.
let (|Singleton|_|) element listarg =
  match listarg with
  | [x]
    -> if x = element
        then Some ()
        else None
  | _ -> None
// [/snippet]


let test1 = [1; 2; 3]
let test2 = []
let test3 = [2]
let test4 = [3]


let two = 2

let r1 = match test1 with
         | Singleton two -> "Matched"
         | _ -> "Not matched"

let r2 = match test2 with
         | Singleton two -> "Matched"
         | _ -> "Not matched"

let r3 = match test3 with
         | Singleton two -> "Matched"
         | _ -> "Not matched"

let r4 = match test4 with
         | Singleton two -> "Matched"
         | _ -> "Not matched"
