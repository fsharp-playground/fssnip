open Microsoft.FSharp.Math
open System.Threading.Tasks
open System.IO

let splitMatrix (original : Matrix<float>) =
    let (oldRows, oldCols) = original.Dimensions
    let newRows = oldRows >>> 1
    let newCols = oldCols >>> 1
    let c11 = original.[0..(newRows - 1), 0..(newCols - 1)]
    let c12 = original.[0..(newRows - 1), newCols..(oldCols - 1)]
    let c21 = original.[newRows..(oldRows - 1), 0..(newCols - 1)]
    let c22 = original.[newRows..(oldRows - 1), newCols..(oldCols - 1)]
    (c11, c12, c21, c22)
    

let integrateMatrix(a11 : Matrix<float>, a12 : Matrix<float>, a21 : Matrix<float>, a22 : Matrix<float>) =
     let (oldRows, oldCols) = a11.Dimensions
     let (rows, cols) = (oldRows <<< 1, oldCols <<< 1)
     let helper i j = 
        if (i < oldRows) then
           if (j < oldCols) then
                a11.[i, j]
           else
               a12.[i, (j - oldCols)] 
        else
            if (j < oldCols) then
                a21.[(i - oldRows), j]
            else
                a22.[(i - oldRows), (j - oldCols)]
     Matrix.init rows cols helper

let rec Strassen (A : Matrix<float>, B : Matrix<float>) =
    let (rows, cols) = A.Dimensions
    if (rows <= 64) then
        A * B
    else
        let (a11, a12, a21, a22) = splitMatrix(A)
        let (b11, b12, b21, b22) = splitMatrix(B)
        let M1 = Task.Factory.StartNew(fun () -> Strassen ((a11 + a22), (b11 + b22)))
        let M2 = Task.Factory.StartNew(fun () -> Strassen ((a21 + a22), b11))
        let M3 = Task.Factory.StartNew(fun () -> Strassen (a11, (b12 - b22)))
        let M4 = Task.Factory.StartNew(fun () -> Strassen (a22, (b21 - b11)))
        let M5 = Task.Factory.StartNew(fun () -> Strassen ((a11 + a12), b22))
        let M6 = Task.Factory.StartNew(fun () -> Strassen ((a21 - a11), (b11 + b12)))
        let M7 = Strassen ((a12 - a22), (b21 + b22))
        let c11 = M1.Result + M4.Result - M5.Result + M7
        let c12 = M3.Result + M5.Result
        let c21 = M2.Result + M4.Result
        let c22 = M1.Result - M2.Result + M3.Result + M6.Result
        integrateMatrix(c11, c12, c21, c22)
