open System

type Calc = Calc of int list

type CalcBuilder() =
    member this.Bind(x, f) = (fun c0 -> let a, Calc c = x c0 in f a c)
    member this.Delay(f) = (fun c0 -> f c0)
    member this.Return(x) = (fun c0 -> x, c0)
    member this.ReturnFrom(x) = x
    member this.Combine(x, f) = this.Bind(x, f)

let calc = new CalcBuilder()

let popCalc = (fun stack -> (List.head stack, Calc <| List.tail stack))
let pushCalc n = (fun stack -> ((), Calc <| n :: stack))
let addCalc = (fun stack -> match stack with
                            | []          -> ((),Calc [])
                            | [a]         -> ((),Calc [a])
                            | a :: b :: c -> ((),Calc ((a + b) :: c)))

let initialCalc = (), []

let func = calc {
    do! pushCalc 3
    do! pushCalc 4
    do! addCalc
    return! popCalc
}