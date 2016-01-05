// Define complex type with some operators
type Complex =
    { Re : float;
      Im : float }
    static member (+) (z1, z2) = 
        { Re = z1.Re + z2.Re; 
          Im = z1.Im + z2.Im }
    static member (-) (z1, z2) = 
        { Re = z1.Re - z2.Re; 
          Im = z1.Im - z2.Im }
    static member (*) (z1, z2) = 
        { Re = ((z1.Re * z2.Re) - (z1.Im * z2.Im));
          Im = ((z1.Re * z2.Im) + (z1.Im * z2.Re)) }
    static member (/) (z1, z2) = 
        let z2_conj = {Re = z2.Re; Im = -z2.Im}
        let den = (z2 * z2_conj).Re
        let num = z1 * z2_conj
        { Re = num.Re / den;
          Im = num.Im / den }
    static member (~-) z = 
        { Re = -z.Re; 
          Im = -z.Im };;

// .. and printing
let print z = printfn "%.3f%+.3fi" z.Re z.Im;;


// Try it (Example 1.3)
let z = {Re = 1.0; Im = 4.0};;
let w = {Re = 2.0; Im = -2.0};;

print (z + w);; // need the brackets !!
print (z * w);;

// Example 1.4 
let z1 = {Re = 1.0; Im = 1.0 };;
let z2 = {Re = 1.0; Im = -2.0 };;

print (z1 / z2);;

// Test unary - operator
let w1 = {Re = 2.0; Im = 3.0};
print (-w);
print (z1 + (-w1));
print (z1 - w1);;
print (z1 * (-w1));
