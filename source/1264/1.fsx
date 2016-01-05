// Pull xBehave from Nuget
open Xbehave
open Xunit

/// Custom operator for step set up
let (-->) (s:string) f = s.f(System.Action< >(f)) |> ignore

// [snippet:SUT]
type Calculator () = member __.Add(x,y) = x + y
// [/snippet]

// [snippet:Quickstart]
let [<Scenario>] addition(x:int,y:int,calculator:Calculator,answer:int) =
    let x,y,calculator,answer = ref x, ref y, ref calculator, ref answer
    "Given the number 1" --> fun () -> x := 1
    "And the number 2" --> fun () -> y := 2
    "And a calculator" --> fun () -> calculator := Calculator()
    "When I add the numbers together" --> fun () -> answer := (!calculator).Add(!x, !y)
    "Then the answer is 3" --> fun () -> Assert.Equal(3, !answer)
// [/snippet]

// [snippet:Scenarios with examples]
[<Scenario>]
[<Example(1, 2, 3)>]
[<Example(2, 3, 5)>]
let adding(x:int,y:int,expectedAnswer:int,calculator:Calculator,answer:int) =
    let calculator, answer = ref calculator, ref answer
    "Given the number {0}" --> id   
    "And the number {1}" --> id
    "And a calculator" --> fun () -> calculator := Calculator()
    "When I add the numbers together" --> fun () -> answer := (!calculator).Add(x, y)
    "Then the answer is {2}" --> fun () -> Assert.Equal(expectedAnswer, !answer)
// [/snippet]