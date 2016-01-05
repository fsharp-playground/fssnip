//-----------------------------------------------------
//First example:

module NumericLiteralN =
    let FromZero() = ""
    let FromOne() = "n"
    let FromInt32 x = String.replicate x "n"

// Calls FromOne():
let x1 = 1N 
// val x1 : string = "n"

// Calls FromInt32(7):
let x2 = 7N
// val x1 : string = "nnnnnnn"

//Calls operator (+) on strings.
let x3 = 2N + 3N
// val x3 : string = "nnnnn"


//-----------------------------------------------------
//Second example:

type MyExpression =
| Const of int
| Plus of MyExpression * MyExpression
| Mult of MyExpression * MyExpression
with 
    static member (+) (x, y) = Plus(x,y)
    static member (*) (x, y) = Mult(x,y)

module NumericLiteralZ =
    let FromZero() = Const 0
    let FromOne() = Const 1
    let FromInt32 = Const

let rec eval tree =
    match tree with
    | Plus (x, y) -> eval x + eval y
    | Mult (x, y) -> eval x * eval y
    | Const x -> x

let expression = 3Z + (4Z * 5Z)
// val expression : MyExpression = Plus (Const 3,Mult (Const 4,Const 5))

let res = eval expression
// val res : int = 23
