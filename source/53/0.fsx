// calculates the factorial:
// n! = 1 * 2 * 3 * ... * n
// the factorial only exists for positive integers
let rec factorial n =
    match n with
    | 0 | 1 -> 1
    | _ -> n * factorial (n - 1)