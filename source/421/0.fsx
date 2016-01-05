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

// .. and the conjugate
let conj z = 
    { Re = z.Re; 
      Im = -z.Im };;

// Problem 1.1
let z = {Re = 4.0; Im = 3.0};;
let w = {Re = 2.0; Im = -5.0};;
let one = {Re = 1.0; Im = 0.0 };;

print (one / z);;

// Check those fractions
printfn "%f %f" (4.0 / 25.0) (3.0 / 25.0);;

print (z * (conj w));;

print ((conj z) * w);;

print ((conj z) / (conj w));;

// Check those fractions
printfn "%f %f" (4.0 / 25.0) (3.0 / 25.0);;
