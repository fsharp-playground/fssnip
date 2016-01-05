module Program

type Card = S | K | I | Zero | Succ | Dbl | Get | Put | Inc | Dec | Attack | Help | Copy | Revive | Zombie
    with
        override x.ToString() =
            match x with
            | S -> "S"
            | K -> "K"
            | I -> "I"
            | Zero -> "zero"
            | Succ -> "succ"
            | Dbl -> "dbl"
            | Get -> "get"
            | Put -> "put"
            | Inc -> "inc"
            | Dec -> "dec"
            | Attack -> "attack"
            | Help -> "help"
            | Copy -> "copy"
            | Revive -> "revive"
            | Zombie -> "zombie"
        static member Parse s =
            match s with
            | "S" -> S
            | "K" -> K
            | "I" -> I
            | "zero" -> Zero
            | "succ" -> Succ
            | "dbl" -> Dbl
            | "get" -> Get
            | "put" -> Put
            | "inc" -> Inc
            | "dec" -> Dec
            | "attack" -> Attack
            | "help" -> Help
            | "copy" -> Copy
            | "revive" -> Revive
            | "zombie" -> Zombie
            | _ -> failwith "Parse error: Invalid card type."

type SKITerm =
    | Card of Card
    | App of SKITerm * SKITerm
    with
        override x.ToString() =
            match x with
            | Card c -> c.ToString()
            | App(m, n) -> m.ToString() + "(" + n.ToString() + ")"

type Instr =
    | LApp of Card * int
    | RApp of int * Card

exception OutOfSpace
exception SlotNotFree

let mutable freeList = [0 .. 255]

let alloc i =
    if List.exists (fun j -> j = i) freeList then
        freeList <- List.filter (fun j -> j <> i) freeList
    else
        raise SlotNotFree

let alloc1() =
    match freeList with
        | x::xs ->
            freeList <- List.filter (fun j -> j <> x) freeList
            x
        | [] -> raise OutOfSpace

let isFree i =
    List.exists (fun j -> j = i) freeList

let free i =
    freeList <- (i::freeList) |> List.sort

type Term =
    | C of Card
    | L of Card * Term
    | R of Term * Card

let rec intToTerm n =
    if n = 0 then
        C Zero
    elif n % 2 = 0 then
        L(Dbl, intToTerm (n/2))
    else
        L(Succ, intToTerm (n-1))

let rec termToSKITerm = function
    | C c -> Card c
    | L(c, t) -> App(Card c, termToSKITerm t)
    | R(t, c) -> App(termToSKITerm t, Card c)

let intToSKITerm n = intToTerm n |> termToSKITerm

let rec flatten i = function
    | C c -> [RApp(i, c)]
    | L(c, t) -> (flatten i t) @ [LApp(c, i)]
    | R(t, c) -> (flatten i t) @ [RApp(i, c)]

let succn s i t =
    let rec helper s n =
        match n with
        | n when n = s -> []
        | 0 -> []
        | 1 -> [Succ]
        | n when n % 2 = 0 && (n/2) >= s -> Dbl :: helper s (n/2)
        | n -> Succ :: helper s (n-1)
    let rec loop l =
        match l with
        | [] -> t
        | x::xs -> R(L(S, L(K, loop xs)), x)
    helper s i |> List.rev |> loop

let apply i j loc =
    if i <= j then failwith "Bug: apply: i <= j"
    R(succn 0 j (R(L(S, succn j i (C Get)), Get)), Zero) |> flatten loc

/// Compile term into instruction list for slot i.
/// Assumes slot i is set to I.
let rec compile s = function
    | App(Card c1, Card c2) -> [RApp(s, c2); LApp(c1, s)]
    | App(Card c, t) -> (compile s t) @ [LApp(c, s)]
    | App(t, Card c) -> (compile s t) @ [RApp(s, c)]
    | App(t1, t2) ->
        let j = alloc1()
        let i = alloc1()
        if i <= j then failwith "Bug: compile: i <= j"
        let code =
            (compile j t2)
            @ (compile i t1)
            @ (R(succn 0 j (R(L(S, succn j i (C Get)), Get)), Zero) |> flatten s)
            @ [LApp(Put, i); LApp(Put, j)]  // Clear fields i, j
        free i
        free j
        code
    | Card c -> [RApp(s, c)]

let zombie3 card i j n = App(App(Card S, App(App(Card S, App(App(Card S, App(Card K, Card card)), App(Card K, intToSKITerm i))), App(Card K, intToSKITerm j))), App(Card K, intToSKITerm n))

type Program() =
//    let init() =
//        List.iter alloc [0; 1; 2; 4]
//        (5556 |> intToTerm |> flatten 0)
//        @ (0 |> intToTerm |> flatten 1)
//        @ (128 |> intToTerm |> flatten 2)
//        @ [RApp(4, Attack)]
//    let attacker i =
//        List.iter alloc [3; 5; 6; 7; 8]
//        
//        let code =
//            (i |> intToTerm |> flatten 3)
//
//            @ (apply 4 1 5)
//            @ (apply 5 3 6)
//            @ (apply 6 0 7)
//
//            @ [LApp(Put, 5)]
//            @ [LApp(Put, 6)]
//
//            @ [RApp(7, Zero)]
//            @ [LApp(Put, 7)]
//
//            @ (apply 4 2 5)
//            @ (apply 5 3 6)
//            @ (apply 6 0 8)
//
//            @ [LApp(Put, 3)]
//
//            @ [LApp(Put, 5)]
//            @ [LApp(Put, 6)]
//
//            @ [RApp(8, Zero)]
//            @ [LApp(Put, 8)]
//
//            @ [LApp(Succ, 1)]
//            @ [LApp(Succ, 2)]
//
//        List.iter free [3; 5; 6; 7; 8]
//        code

    let init() =
        List.iter alloc [0; 1; 2; 4; 9]
        (5556 |> intToTerm |> flatten 0)
        @ (0 |> intToTerm |> flatten 1)
        @ (128 |> intToTerm |> flatten 2)
        @ [RApp(4, Attack)]

        // Zombie
        @ [RApp(9, Zombie)]
    let attacker i =
        List.iter alloc [3; 5; 6; 7; 8; 11; 12]
        
        let code =
            (i |> intToTerm |> flatten 3)

            @ (apply 4 1 5)
            @ (apply 5 3 6)
            @ (apply 6 0 7)

            @ [LApp(Put, 5)]
            @ [LApp(Put, 6)]

            @ [RApp(7, Zero)]
            @ [LApp(Put, 7)]

            @ (apply 4 2 5)
            @ (apply 5 3 6)
            @ (apply 6 0 8)

            @ [LApp(Put, 5)]
            @ [LApp(Put, 6)]

            @ [RApp(8, Zero)]
            @ [LApp(Put, 8)]

            // Zombie
            @ ((zombie3 Help i i 10000) |> compile 10)
            @ (apply 9 3 11)
            @ (apply 11 10 12)
            @ [LApp(Put, 10)]
            @ [LApp(Put, 11)]
            @ [RApp(12, Zero)]
            @ [LApp(Put, 12)]
            // End Zombie

            @ [LApp(Put, 3)]

            @ [LApp(Succ, 1)]
            @ [LApp(Succ, 2)]

        List.iter free [3; 5; 6; 7; 8; 11; 12]
        code

    let mutable code = init()
    let mutable i = 0
    let mutable strategy = 0

    member x.NextInstr() =
        match code with
        | x::xs -> code <- xs; x
        | [] ->
            if strategy = 0 then
                code <- attacker i
                if i = 255 then strategy <- strategy + 1
                i <- i + 1
            if strategy = 1 then
                code <- [RApp(0, I)]
            x.NextInstr()

module IO =
    let readInt() =
        try
            System.Console.ReadLine().Trim() |> System.Int32.Parse
        with _ -> exit 0

    let readCard() =
        try
            System.Console.ReadLine().Trim() |> Card.Parse
        with _ -> exit 0

    let readInstr() =
        match readInt() with
        | 1 ->
            let card = readCard()
            let slot = readInt()
            LApp(card, slot)
        | 2 ->
            let slot = readInt()
            let card = readCard()
            RApp(slot, card)
        | _ -> failwith "Invalid left/right-application specifier."

    let writeInstr = function
        | LApp(card, slot) ->
            System.Console.WriteLine(1)
            System.Console.WriteLine(card)
            System.Console.WriteLine(slot)
        | RApp(slot, card) ->
            System.Console.WriteLine(2)
            System.Console.WriteLine(slot)
            System.Console.WriteLine(card)

let prog = Program()

let rec player() =
    prog.NextInstr() |> IO.writeInstr
    opponent()

and opponent() =
    let instr = IO.readInstr()
    player()

[<EntryPoint>]
let main _ =
    let arg1 = System.Environment.GetCommandLineArgs().[1] |> System.Int32.Parse
    match arg1 with
    | 0 -> player()
    | 1 -> opponent()
    | _ -> failwith "Invalid argument."
