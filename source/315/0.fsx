let dbgFunction d = 
    printfn "[Beginning debug operation]"

    printfn "test %d" d

    printfn "[Debug operation complete]"

let stndFunction d = 
    printfn "test %d" d    
    
let printft v d = 
    if v < 3 then stndFunction d
    else dbgFunction d
    
printft 2 148418
printft 3 148418