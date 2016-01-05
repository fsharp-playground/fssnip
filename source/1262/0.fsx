open Xbehave
open Xunit

type Calculator () = member __.Add(x,y) = x + y

let [<Scenario>] addition(x:int,y:int,calculator:Calculator,answer:int) =
    let x,y,calculator,answer = ref x, ref y, ref calculator, ref answer    
    "Given the number 1"
        .Given(fun () -> x := 1) |> ignore
    "And the number 2"
        .And(fun () -> y := 2) |> ignore
    "And a calculator"
        .And(fun () -> calculator := Calculator()) |> ignore
    "When I add the numbers together"
        .When(fun () -> answer := (!calculator).Add(!x, !y)) |> ignore
    "Then the answer is 3"
        .Then(fun () -> Assert.Equal(3, !answer))
