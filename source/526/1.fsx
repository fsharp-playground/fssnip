(*[omit:(helper functions omitted)]*)
open System
let readln () = Console.ReadLine()
let readkey () = Console.ReadKey()
let tryParseInt = Int32.TryParse 
let using color f = 
    Console.ForegroundColor <- color
    f ()
    Console.ResetColor ()
let red = ConsoleColor.Red
let green = ConsoleColor.Green
let beep () = Console.Beep ()
let wait (n:int) = Threading.Thread.Sleep n
let now () = DateTime.Now
let rand = Random()(*[/omit]*)

let play times = 
    for message in ["Get Ready";"3";"2";"1";"Go"] do 
        printfn "%s" message
        wait 1000
    let begun = now ()
    [1..times] |> Seq.sumBy (fun i ->
        let a, b = rand.Next 13, rand.Next 13
        printf "%d x %d = " a b
        let entered = readln ()
        match tryParseInt entered with
        | true, answer when answer = a * b -> 
            using green (fun () -> printfn "Correct")
            1
        | _, _ ->
            beep ()
            using red (fun () -> printfn "%d x %d = %d" a b (a*b))
            0
    ) |> (fun score ->
        let taken = (now() - begun).ToString("c")
        printfn "%d correct out of %d in %s" score times taken
    )

while true do 
    play 20
    wait 5000
    printfn "Hit a key to play again?"
    readkey () |> ignore