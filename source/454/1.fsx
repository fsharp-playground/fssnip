let quicksort l = 
    printfn "quick sort"
let shellsort l = 
    printfn "shell short"
let bubblesort l = 
    printfn "bubble sort"
type Strategy(sortFunction) = 
    member this.SortFunction with get() = sortFunction    
    member this.Execute(list) = sortFunction list

let strategy() = 
    let s = Strategy(quicksort)    
    s.Execute([1..6])

strategy()