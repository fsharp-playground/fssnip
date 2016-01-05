let quicksort l = 
    printfn "quick sort"
let shellsort l = 
    printfn "shell short"
let bubblesort l = 
    printfn "bubble sort"
type Strategy() = 
    let mutable sortFunction = fun _ -> ()
    member this.SetStrategy(f) = sortFunction <- f
    member this.Execute(n) = sortFunction(n)

let stragegy() = 
    let s = Strategy()
    s.SetStrategy(quicksort)
    s.Execute([1..6])