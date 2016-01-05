type AccountState = 
    | Overdrawn
    | Silver
    | Gold
[<Measure>] type USD
type Account<[<Measure>] 'u>() =
    let mutable balance = 0.0<_>   
    member this.State
        with get() = 
            match balance with
            | _ when balance <= 0.0<_> -> Overdrawn
            | _ when balance > 0.0<_> && balance < 10000.0<_> -> Silver
            | _ -> Gold
    member this.PayInterest() = 
        let interest = 
            match this.State with
                | Overdrawn -> 0.
                | Silver -> 0.01
                | Gold -> 0.02
        interest * balance
    member this.Deposit(x:float<_>) =  
        let (a:float<_>) = x
        balance <- balance + a
    member this.Withdraw(x:float<_>) = balance <- balance - x

let state() = 
    let account = Account()

    account.Deposit(10000.<USD>)
    printfn "interest = %A" (account.PayInterest())

    account.Withdraw(20000.<USD>)
    printfn "interest = %A" (account.PayInterest())
