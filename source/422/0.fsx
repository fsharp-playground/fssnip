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

// ... and the modulus (absolute value)
let abs z =
    sqrt (z.Re * z.Re + z.Im * z.Im);;

// ... and the argument (actually this is the principal value of the argument (Arg)
let arg z = 
    atan2 z.Im z.Re;;

// Polar form of complex number
type ComplexPolar = 
    { Mag : float;
      Arg : float };;

// ... with conversion to and from the polar form
let toPolar z = 
    { Mag = abs z;
      Arg = arg z };;

let fromPolar zp = 
    { Re = zp.Mag * (cos zp.Arg);
      Im = zp.Mag * (sin zp.Arg) };;

// ... and define printing of the polar form
let printp zp = 
    printfn "%.1f(cos %.3f + i sin %.3f)" zp.Mag zp.Arg zp.Arg;;

// Try it
let i = {Re = 0.0; Im = 1.0};;
printp (toPolar i);;

let z1 = {Re = 3.0; Im = 4.0};;
let z2 = fromPolar (toPolar z1);;
print z1;;
print z2;; // should be equal to z1.

// Example 2.1
let z = {Re = -1.0; Im = -1.0};;
let zp = toPolar z;;
printp zp;;

// ...how to check those decimals?
let pi = atan2 0.0 -1.0;;
printfn "%f" (- 3.0 * pi / 4.0);; 
