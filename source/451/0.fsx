type Record = {
    Name : string;
    Age : int;
    Weight: float;
    Height: float;
}

let ChainOfResponsibility() = 
    let validAge (record:Record) = 
        record.Age < 65 && record.Age > 18
    let validWeight (record:Record) = 
        record.Weight < 200.
    let validHeight (record:Record) = 
        record.Height > 120.

    let check (f:Record->bool) (record:Record, result:bool) = 
        if result=false then (record, false)
        else (record, f(record))

    let chainOfResponsibility = check(validAge) >> check(validWeight) >> check(validHeight)

    let john = { Name = "John"; Age = 80; Weight = 180.; Height=180. }
    let dan = { Name = "Dan"; Age = 20; Weight = 160.; Height=190. }

    printfn "john result = %b" ((chainOfResponsibility (john, true)) |> snd)
    printfn "dan result = %b" ((chainOfResponsibility (dan, true)) |> snd)
