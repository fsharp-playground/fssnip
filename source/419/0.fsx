// Define the Complex type as a record
type Complex = 
    { Re : float
      Im : float };;

// Make some complex numbers
let z1 = {Re = 1.0; Im = 4.0};;
let z2 = {Re = 2.0; Im = -2.0};;

// printing strings are OK
printfn "Hello";;
// Error - don't know how to print a Complex
//printfn z1;; // <-- Error 

// print anything with '%A'
printfn "%A" z1;;

// Make our own printing for Complex (using float formatting %f, 3 decimals)
let print z = printfn "%.3f%+.3fi" z.Re z.Im;;
 
// ... and try it out
print z1;;