type CoreComputation() = 
    member this.Add(x) = x + 1
    member this.Sub(x) = x - 1
    member this.GetProxy(name) = 
        match name with
        | "Add" -> (this.Add, "add")
        | "Sub" -> (this.Sub, "sub")
        | _ -> failwith "not supported"

let proxy() = 
    let core = CoreComputation()
    let proxy = core.GetProxy("Add")
    printfn "result = %d" ((fst proxy) 1)
