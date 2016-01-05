// Gray-Code
// "a binary numeral system where two successive values differ in only one bit"  http://en.wikipedia.org/wiki/Gray_code
//
// http://FunctionalSoftware.net/starten-mit-f/
let (++) x = List.map((+)x)
let g x = ("0" ++ x) @ ("1" ++ (List.rev x))
printf "%A" ([""] |> g |> g |> g)
// ["000"; "001"; "011"; "010"; "110"; "111"; "101"; "100"]