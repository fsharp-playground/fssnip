let binaryOperator operator val1 val2
    = operator val1 val2
    
let rec add x y =
    if( y > 0 ) then
        (add x (y - 1)) + 1
    else
        x
    
let rec sub x y =
    if( y > 0 ) then
        (sub x (y - 1)) - 1
    else
        x
    
let rec mul x y =
    if( y > 0 ) then
        add x (mul x (y - 1))
    else
        0
    
let rec div x y =
    if( x > y ) then
        div (sub x y) y
    else
        1
    
let rec exp x y =
    if( y > 0 ) then
        mul x (exp x (y - 1))
    else
        1
        
// Test our defined operators
printfn "%d" (add 7 2)
printfn "%d" (sub 7 2)
printfn "%d" (mul 7 2)
printfn "%d" (div 1000 30)
printfn "%d" (exp 7 2)
        
// Mutable functions
let mutable f = (fun x y -> x + y)
printfn "%d" (f 7 2)

f <- (fun x y -> x * y)
printfn "%d" (f 7 2)

// Higher order functions
printfn "%d" (binaryOperator (fun x y -> exp (add x y) (div x y)) 3 1)