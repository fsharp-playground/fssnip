let (|Even|Odd|) i = if i % 2 = 0 then Even else Odd


let TestNumber input =
   match input with
   | Even -> printfn "%d is even" input
   | Odd -> printfn "%d is odd" input

TestNumber 7
TestNumber 11
TestNumber 32