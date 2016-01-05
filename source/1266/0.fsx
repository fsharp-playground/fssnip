open Xbehave
open Xunit

// [snippet:SUT]
type Calculator () = member __.Add(x,y) = x + y
// [/snippet]

// [snippet:F# computation builder]
type StepBuilder(msg:string) = 
  member x.Zero() = ()
  member x.Delay(f) = f
  member x.Run(f) = msg.f(System.Action< >(f)) |> ignore

let Given what = StepBuilder("Given " + what)
let And what = StepBuilder("And " + what)
let When what = StepBuilder("When " + what)
let Then what = StepBuilder("Then " + what)
// [/snippet]

// [snippet:Sample scenario]
let [<Scenario>] addition() = 
  let x, y, calculator, answer = ref 0, ref 0, ref (Calculator()), ref 0
  Given "the number 1" { x := 1 }
  And "the number 2" { y := 2 }
  And "a calculator" { calculator := Calculator() }
  When "I add the numbers together" {
    answer := (!calculator).Add(!x, !y) }
  Then "the answer is 3" { Assert.Equal(3, !answer) }
// [/snippet]