// F# has lists, which we'll use often

// a list of floats 
let list1 = [1.0; 2.0; 5.0];;

// Make a list of integers from 1 to 4
let list2 = [1 .. 4];;

// Making a list of floats from 1.0 to 3.0, with step size 0.2
let list3 = [1.0 .. 0.2 .. 3.0];;

// map allows you to aply a function to each element in a list
let list4 = 
    List.map (fun x -> 2 * x) list2;;

// Shortcut to pi
let pi = atan2 0.0 -1.0 
    
// Make the list of fractions for roots on p.14
// So we want 
//    theta + 2 * k * pi / n where k = 0... n-1
// Now we will take angles mod 2 * pi, so they're in the range [0...2 * pi)
// And sort them from small to large.
let rootAngles theta n =
    let kList = [0 .. (n-1)]
    let angles = List.map (fun k -> (theta + 2.0 * (float k) * pi) / (float n)) kList
    let anglesModPi = List.map (fun angle -> angle % (2.0 * pi)) angles
    let anglesSorted = List.sort anglesModPi
    anglesSorted;;

// Test a list of angles
rootAngles pi 4;;

/////////////////////////////////////////////////////////////////////////
// Back to Complex numbers - define type and operators

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

/////////////////////////////////////////////////////

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

// Example 2.6
let z = {Re = 1.0; Im = 0.0};;

let roots = nthRoots 8 z;;

// Print out the list (apply the action to every item using iter)
List.iter print roots;;

////////////////////

// Example 2.7
let w = {Re = -16.0; Im = 0.0};;

// ... in polar
let wp = toPolar w;;
let wRoots = nthRootsPolar 4 w;;
List.iter printp wRoots;;

List.iter print (nthRoots 4 w);;
