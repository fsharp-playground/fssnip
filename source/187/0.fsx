type Ast
  = Identifier of string
  | Number of int

let num f x = f x

num Number 1