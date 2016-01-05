// [snippet:Helpers]
module Helpers =
    let curry f a b = f (a,b)
    let uncurry f (a,b) = f a b
// [/snippet]

// [snippet:Vector-type]
/// Represent a vector as an float array
type Vector =  | Vector of float array
    with 
    /// shortcut to access the float array
    member private v.Values = match v with Vector v' -> v'
    /// access a component of the vector
    member v.Item(i) = v.Values.[i]
    /// the vectors dimension = nr of components
    member v.Dim = Array.length v.Values

    // your normal vector-operators
    static member (+) (a : Vector, b : Vector) = Array.zip a.Values b.Values |> Array.map (Helpers.uncurry (+)) |> Vector
    static member (-) (a : Vector, b : Vector) = Array.zip a.Values b.Values |> Array.map (Helpers.uncurry (-)) |> Vector
    static member (*) (s : float, a : Vector) = a.Values |> Array.map ((*) s) |> Vector

    /// computes the inner product (also known as scalar product) of two vectors
    static member InnerProd (a : Vector, b : Vector) = Array.zip a.Values b.Values |> Array.sumBy (Helpers.uncurry (*))
    static member (.*.) (a,b) = Vector.InnerProd(a,b)

    /// the squared length of a vector
    static member Len2 (a : Vector) = a .*. a
    /// the length of a vector
    static member Len = System.Math.Sqrt << Vector.Len2
    member v.Length = Vector.Len v

    /// normalize a vector (the result will have the same direction but length 1)
    static member Normalize (a : Vector) = (1.0 / a.Length) * a
    member v.Normal = Vector.Normalize v

    /// create a zero-vector
    static member Zero(n) = Array.zeroCreate n |> Vector
// [/snippet]

// [snippet:Matrix - type]
/// represent a matrix as a 2-dimensional float array
type Matrix = Matrix of float [,]
    with
    /// shortcut to the array
    member private m.Values = match m  with Matrix m' -> m'
    
    /// the number of rows
    member m.NrRows = Array2D.length1 m.Values
    /// the number of columns
    member m.NrColumns = Array2D.length2 m.Values

    /// access a column-vector via its zero-based index
    member m.Column(i) = [| 0..Array2D.length1 m.Values-1 |] |> Array.map (fun z -> m.Values.[z,i]) |> Vector

    /// get all column-vectors as an array
    member m.Columns = [|0..Array2D.length2 m.Values-1 |] |> Array.map m.Column
    static member ColVectors (m : Matrix) = m.Columns

    /// Matrix-multiplication
    static member (*) (a : Matrix, b : Matrix) =
        Array2D.init 
            a.NrRows b.NrColumns
            (fun r c -> [0..a.NrColumns-1] 
                        |> List.map (fun i -> a.Values.[r,i] * b.Values.[i,c])
                        |> List.sum)
        |> Matrix
        
    /// Matrix-addition
    static member (+) (a : Matrix, b : Matrix) =
        Array2D.init 
            a.NrRows b.NrColumns
            (fun r c -> a.Values.[r,c] + b.Values.[r,c])
        |> Matrix
        
    /// Matrix-substraction
    static member (-) (a : Matrix, b : Matrix) =
        Array2D.init 
            a.NrRows b.NrColumns
            (fun r c -> a.Values.[r,c] + b.Values.[r,c])
        |> Matrix
        
    /// scalar - multiplication
    static member (*) (s : float, b : Matrix) =
        Array2D.init 
            b.NrRows b.NrColumns
            (fun r c -> s * b.Values.[r,c])
        |> Matrix
        

    /// computes the transpose of a matrix
    static member Transpose (m : Matrix) = Array2D.init m.NrRows m.NrColumns (fun x y -> m.Values.[y,x]) |> Matrix

    /// creates a matrix from a array of columnvectors
    static member Create (vs : Vector array) =
        let l = vs.[0].Dim
        Array2D.init l vs.Length (fun r c -> vs.[c].[r])
        |> Matrix

    /// creates a matrix with an generator-function (the row r and col c of the matrix will have a value of f(r,c))
    static member Create (rows, cols, f) =
        Array2D.init rows cols f
        |> Matrix

    /// creates a zero-Matrix
    static member Zero (rows, cols) = Array2D.zeroCreate rows cols 
// [/snippet]

// [snippet:Gram-Schmidt method]
/// computes a set of orthogonal or orthonormal vectors that spans the same
/// subspace as the given vector-set
module GramSchmidt =

    /// projects a vector onto another vector
    let private project (baseV : Vector) (v : Vector) : Vector 
        = (v .*. baseV)/(baseV .*. baseV) * baseV

    /// computes a set of orthogonal vectors that span the same subspace as the vectors in vs
    /// see the Wikipedia-article at http://en.wikipedia.org/wiki/Gram_schmidt for a good
    /// explanation of this method
    let Orthogonalize (vs : Vector list) =
        // computes one vector based on the allready found vectors
        let rec calcNextVec cur found =
            match found with
            | [] -> cur
            | v::vs' -> calcNextVec (cur - project v cur) vs'
        // computes the vectors
        let rec calcVecs toDo found =
            match toDo with
            | [] -> found
            | cur::rest -> calcVecs rest ((calcNextVec cur found)::found)
        calcVecs vs [] |> List.rev

    /// computes a set of orthonormal vectors that span the same subspace as the vectors in vs
    let OrthogonalizeNormal = Orthogonalize >> List.map Vector.Normalize
// [/snippet]

// [snippet:QR-decomposition]
/// decomposes a square matrix into a product of an orthongal and an upper triangular matrix
module QRdecomposition =
    
    /// computes the QR-decomposition using the Gram-Schmidt method
    /// see the Wikipedia-article at http://en.wikipedia.org/wiki/QR_decomposition for a good
    /// explanation of this method
    let UsingGramSchmidt (m : Matrix) =
        let colVs = m.Columns
        let normVs = Array.ofList <| GramSchmidt.OrthogonalizeNormal (List.ofArray colVs)
        let Q = Matrix.Create normVs
        let f r c = if r <= c then normVs.[r] .*. colVs.[c] else 0.0
        let R = Matrix.Create (m.NrRows, m.NrColumns, f)
        Q,R
// [/snippet]