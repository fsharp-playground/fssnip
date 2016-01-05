// Option 1: binds
// Not much different to your solution, but slightly less repetitive
let (+.) x y =
    Option.bind (fun x' -> Option.bind (fun y' -> Some (x' + y')) y) x

(Some 10) +. (Some 20)

[
    Some 10
    Some 5
    Some 6
]
|> List.reduce (+.)

// Option 2: With maybe monad from Fsharpx
open FSharpx.Option

let (+..) x y =
    maybe {
        let! x' = x
        let! y' = y
        return x' + y'
    }
