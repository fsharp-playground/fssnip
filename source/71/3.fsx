// [snippet:Declarations]
// The const function
let ct a _ = a

// The flip function (not used in this snippet)
let flip f x y = f y x
// [/snippet]
// [snippet:Use case]
// Code taken from a toy interpreter

type Exp = 
    // ...
    // Lambdas are expressed as F# an function that takes a list of Exps and an env
    | Lambda of (Exp list -> Env -> Exp)
    // ...
and Env = (string * Exp) list

// Built-ins receive evaluated arguments so they don't care about the env.
// By composing the function we get with ct we're basically converting a function
// of arity 1 to a function of arity 2 that just ignores its second argument

let builtin f = Lambda (f >> ct)
// [/snippet]