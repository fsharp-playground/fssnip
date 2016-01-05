// meaningless opens
open System
open System.Collections.Generic

// [snippet:FizzBuzz sample]
let fizzbuzz x =
    if x % 3 = 0 && x % 5 = 0 then
        (*[omit:print "fizz" ans "buzz"]*)printfn "fizzbuzz"(*[/omit]*)
    elif x % 3 = 0 then
        (*[omit:print "fizz"]*)printfn "fizz"(*[/omit]*)
    elif x % 5 = 0 then
        (*[omit:print "buzz"]*)printfn "buzz"(*[/omit]*)
    else
        (*[omit:print number]*)printfn "%d" x(*[/omit]*)
 
let fizzbuzz_loop n =
    List.iter fizzbuzz [1 .. n]
// [/snippet] 

// [snippet:main]
fizzbuzz_loop 100;;
// [/snippet]
