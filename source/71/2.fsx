// The const function
let ct a _ = a

// The flip function; even though I'm not gonna use it here
let flip f x y = f y x

// And now a use case 

// I'm writing a toy interpreter so the following code is taken straight 
// from that

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