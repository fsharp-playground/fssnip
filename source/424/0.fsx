// Basic Complex type definitions

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
let sprint z = sprintf "%.3f%+.3fi" z.Re z.Im;;

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

// Get list of angles used for roots
let rootAngles theta n =
    let pi = atan2 0.0 -1.0
    let kList = [0 .. (n-1)]
    let angles = List.map (fun k -> (theta + 2.0 * (float k) * pi) / (float n)) kList
    let anglesModPi = List.map (fun angle -> angle % (2.0 * pi)) angles
    let anglesSorted = List.sort anglesModPi
    anglesSorted;;

// Find roots
let nthRootsPolar n z = 
    let zp = toPolar z
    let angles = rootAngles zp.Arg n
    let mag = System.Math.Pow(zp.Mag, (1.0 / (float n)))
    List.map (fun angle -> {Mag = mag; Arg = angle}) angles;;

// ... one way to convert a list from polar
let fromPolarList polars = 
    List.map fromPolar polars;;

// ... another way...
// send (pipe) the output from the nthRootsPolar 
// to a list conversion
let nthRoots n z = 
    nthRootsPolar n z
    |> List.map fromPolar;;

///////////////////////////////////////////////////////////

// Problem 2.6

let test z =
    let one = {Re = 1.0; Im = 0.0}
    let zplus = z + one
    let zmin = z - one
    let absplus = abs zplus
    let absmin = abs zmin
    if absplus > absmin then
        printfn "%s is %s" (sprint z) "OK (satisfies the ineqality)"
    else
        printfn "%s is %s" (sprint z) "BAD (does not satisfy the inequality)"

test {Re = 3.0; Im = -0.5};;
test {Re = -2.0; Im = 4.5};;