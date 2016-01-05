open Xbehave
open Xunit

let (-->) (s:string) f = s.f(System.Action< >(f)) |> ignore

type Calculator () = member __.Add(x,y) = x + y

let [<Scenario>] addition(x:int,y:int,calculator:Calculator,answer:int) =
    let x,y,calculator,answer = ref x, ref y, ref calculator, ref answer    
    "Given the number 1" --> fun () -> x := 1
    "And the number 2" --> fun () -> y := 2
    "And a calculator" --> fun () -> calculator := Calculator()
    "When I add the numbers together" --> fun () -> answer := (!calculator).Add(!x, !y)
    "Then the answer is 3" --> fun () -> Assert.Equal(3, !answer)