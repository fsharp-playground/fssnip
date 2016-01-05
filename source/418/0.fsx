// Define complex type with a '+' operator
type Complex =
    { Re : float;
      Im : float }
    static member (+) (z1, z2) = 
        { Re = z1.Re + z2.Re; 
          Im = z1.Im + z2.Im };;

// .. and printing
let print z = printfn "%.3f%+.3fi" z.Re z.Im;;

// Test it
let z1 = {Re = 1.0; Im = 4.0};;
let z2 = {Re = 2.0; Im = -2.0};;

let z3 = z1 + z2;;

print z3;;
