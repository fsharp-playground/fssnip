// [snippet:Definition of a polynom]

/// a Polynom is just a list of coefficients
/// mind that [a; b; c] correspondents to a*X^0 + b*X^1 + c*X^2
type Polynom<'a> = 'a list
// [/snippet]

// [snippet:Polynom homomorphisms]
module Polynoms =
    
    /// use the power of inline and List.fold to implement a simple polynom eval function for generic polynoms
    let inline Eval (p : 'a Polynom) (v : 'b) =
        p |> List.fold (fun (x, sum) coeff -> 
                        (x*v), (sum + x*coeff)) 
                       (Microsoft.FSharp.Core.LanguagePrimitives.GenericOne,
                        Microsoft.FSharp.Core.LanguagePrimitives.GenericZero)
          |> snd

    /// returns a the polynom-function for a polynom
    let inline CreatePolynomFunction p = Eval p

    /// returns a function that evaluates every given Polynom p at the point v
    let inline EvalHom v = fun p -> Eval p v
// [/snippet]

// [snippet:Some samples with float]
module Samples =
    
    // define a simple Polynom
    let P : Polynom<_> = [4.0; 1.0; 2.0] // 2.0*X^2 + X + 4.0

    // create the polynomfunction and get some values
    let p = Polynoms.CreatePolynomFunction P
    let c0 = p 0.0
    let c1 = p 1.0
    let c2 = p 2.0

    // play with the EvalHom
    // check the "get the coefficient for X^0"-Hom
    let c0hom = Polynoms.EvalHom 0.0
    let c0' = c0hom P
    System.Diagnostics.Debug.Assert((c0 = c0'))

    // get the sum of all coefficients (nice trick - evaluate at 1)
    let coeffSum = Polynoms.EvalHom 1.0
    let c1' = coeffSum P
    System.Diagnostics.Debug.Assert((c1 = c1'))
// [/snippet]