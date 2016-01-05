namespace Talk

open System
open System.Threading

(* PATTERN MATCHING *)

module Example1 =
    let mutable message = null
    let key = Console.ReadKey().KeyChar

    if key = 'Q' then
        message <- "Quit!"
    elif key = '?' then
        message <- "Help!"
    else
        message <- sprintf "You hit %c" key

    printfn "%s" message


module Example2 =
    let processArray (arr : int array) = 
        let mutable result = null

        if arr <> null then
            if arr.Length = 1 && arr.[0] = 42 then
                result <- "1 element, 42"
            elif arr.Length = 3 then
                let x = arr.[0]
                let y = arr.[1]
                if x < y then
                    result <- sprintf "%d < %d" x y
                else
                    let z = arr.[2]
                    result <- sprintf "3 elements, last is %d" z
        if result = null then
            result <- sprintf "%A" arr

        result


(* ACTIVE PATTERNS *)

module Example3 =
    let describeNumber n =
        match n with
        // 0, 1, and negatives are not considered prime or composite
        | 0
        | 1
        | _ when n < 0 ->
            printfn "n is neither prime nor composite"
        // process positive numbers
        | _ ->
            let mutable factors = []
            let mutable remaining = n
            let mutable divisor = 2

            // build up list of factors
            while remaining > 1 do
                if remaining % divisor = 0 then
                    factors <- divisor :: factors
                    remaining <- remaining / divisor
                else
                    divisor <- divisor + 1

            // if only 1 factor, it's prime.  otherwise, composite
            match factors.Length with
            | 1 -> printfn "n is prime"
            | _ -> printfn "n in composite, with factors %A" factors


(* RECURSIVE LOOPS *)

module Example4 =
    (**** these work ok, but can be implemented with recursion ****)

    // 'for' loops
    for i = 1 to 5 do
        printfn "%d" i

    for i in 10 .. -2 .. 1 do
        printfn "%d" i
    
    // 'foreach' loop
    for i in [1; 3; 5; 9] do
        printfn "%d" i

    // 'while' loop, no break
    let start = DateTime.Now
    while (DateTime.Now - start) < TimeSpan.FromSeconds(2.) do
        printfn "Looping..."
        Thread.Sleep(300)


module Example5 =
    (*** these are more awkward ***)

    // 'while' loop with break
    let rnd = Random()
    let start = DateTime.Now
    let mutable keepLooping = true
    while keepLooping && ((DateTime.Now - start) < TimeSpan.FromSeconds(2.)) do
        printfn "Looping..."
        Thread.Sleep(300)
        if rnd.Next() % 10 = 0 then
            keepLooping <- false  // no 'break' keyword


    // 'for' loop with non-constant increment
    //     not possible with built-in 'for' loops


module Example6 = 

    let factorPositive n =         
         let mutable remaining = n
         let mutable factors = []
         let mutable divisor = 2

         // build up list of factors
         while remaining > 1 do
             if remaining % divisor = 0 then
                 factors <- divisor :: factors
                 remaining <- remaining / divisor
             else
                 divisor <- divisor + 1

         factors

(* HIGHER-ORDER FUNCTIONS FOR COLLECTION PROCESSING *)

module Example7 = 
    // map
    let myArray = [|"one"; "two"; "three"|]    
    let stringLengths : int array = 
        Array.zeroCreate myArray.Length
    for i = 0 to myArray.Length - 1 do
        stringLengths.[i] <- myArray.[i].Length


module Example8 = 
    // fold
    let myArray = [|"one"; "two"; "three"|]
    let mutable sumOfStringLengths = 0
    for s in myArray do
        sumOfStringLengths <- sumOfStringLengths + s.Length


module Example9 = 
    // filter
    let myArray = [|"one"; "two"; "three"|]
    let filtered =
        let temp = ResizeArray<string>()
        for s in myArray do
            if s.Length = 3 then
                temp.Add(s)
        temp.ToArray()
        
module Example10 =
    // iter
    let myArray = [|"one"; "two"; "three"|]
    for s in myArray do
        let length = s.Length
        if length = 3 then
            printfn "Length of %A is %d" s length