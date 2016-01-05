// [snippet: FizzBuzz Exibit A (Page 8)]
// Exhibit A, in some cases, performs the `mod`3 and `mod`5 tests more than once
let ``fizzbuzz Exibit A`` n = 
    if n % 3 = 0 && n % 5 = 0 then
        "fizzbuzz"
    elif n % 3 = 0 then
        "fizz"
    elif n % 5 = 0 then
        "buzz"
    else
        string n
// [/snippet]

// [snippet: FizzBuzz Exibit B (Page 8)]
// Exhibit B disperses the buzzing code into more than one place in the program.
let ``fizzbuzz Exibit B`` n =
    if n % 3 = 0 then
        "fizz" + if n % 5 = 0 then
                    "buzz"
                 else
                    ""
    elif n % 5 = 0 then
            "buzz"
    else
        string n
// [/snippet]

// [snippet: FizzBuzz Exibit C (Page 9)]
// Exibit C looks simple an elegant, but it has to check if the string is empty with the (<<|) operator.
let (<<|) a b = if a = "" then b else a
let ``fizzbuzz Exibit C`` n = 
    ((if n % 3 = 0 then "fizz" else "")
    + if n % 5 = 0 then "buzz" else "")
     <<| string n
// [/snippet]

// [snippet: Direct Definition (Page 11-12)]
// DSL implementation
type Cmd = Skip | Halt | Print of string
type Program = Cmd list

let rec interp (xs : Program) = 
    match xs with
        | Skip::t -> interp t
        | Halt::_ -> ""
        | Print s::t -> s + interp t
        | [] -> "" 

type Cont = Program -> Program

let fizz n : Cont = 
    match n with
        | _ when n % 3 = 0 -> fun x -> [Print "fizz"] @ x @ [Halt]
        | _ -> id

let buzz n  : Cont =  
    match n with
        | _ when n % 5 = 0 -> fun x -> [Print "buzz"] @ x @ [Halt]
        | _ -> id

let base' n : Cont = fun x -> x @ [Print (string n)]

let fb n = (base' n << fizz n << buzz n ) [Skip]

let fizzbuzz' n = fb n |> interp
// [/snippet]

// [snippet: Interpretation is a fold (Page 12)]
// First, we notice a known pattern here:
// interp is a fold. We can rewrite it as follows:
let step cmd t =
    match cmd with
        | Skip -> t
        | Halt -> ""
        | Print s -> s + t

let interp' xs = List.foldBack step xs ""

// Next version of Skip-Halt-Print commands (Page 13)
let ``const`` a _ = a

type Program' = string -> string

let skip : Program' = id
let halt  : Program' = ``const`` ""
let print : string -> Program' = (+)

// print "hello" << skip  << print "world" << halt

// (print "hello" << skip  << print "world" << halt) "" = "helloworld"

// We need to accordingly adjust the bodies of our contexts: (Page 14)
type Cont' = Program' -> Program'

let fizz' n : Cont' = 
    match n with
        | _ when n % 3 = 0 -> fun x -> print "fizz" << x << halt
        | _ -> id

let buzz' n : Cont' = 
    match n with
        | _ when n % 5 = 0 -> fun x -> print "buzz" << x << halt
        | _ -> id

let base'' n : Cont' = fun x -> x << print (string n) 

let fizzbuzz'' n = (base'' n << fizz' n << buzz' n) skip ""
// [/snippet]

// [snippet: Inlining (Page 14)]
let fizzbuzz''' n =
    let fizz = 
        match n with
            | _ when n % 3 = 0 -> fun x -> ``const`` ("fizz" + x "")
            | _ -> id
    let buzz = 
        match n with
            | _ when n % 5 = 0 -> fun x -> ``const`` ("buzz" + x "")
            | _ -> id
    (fizz << buzz) id (string n)
// [/snippet]

// [snippet: Final polishing (Page 14)]
let fizzbuzz n = 
    let test d s x =
        match n with
            | _ when n % d = 0 -> fun _ -> s + x ""
            | _ -> x
    (test 3 "fizz" << test 5 "buzz") id <| string n
// [/snippet]
