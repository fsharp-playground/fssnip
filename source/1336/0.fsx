// Add references to NetOffice:

#if INTERACTIVE
#r "NetOffice.dll"
#r "OfficeApi.dll"
#r "VBIDEApi.dll"
#r "ExcelApi.dll"
#endif


open NetOffice
open NetOffice.ExcelApi
open NetOffice.ExcelApi.Enums

/// Helper function to return cell content as float if possible, if not as 0.0.
let cellDouble (cell : obj) = 
    match cell with
    | :? double as _double -> _double
    | _ -> 0.0

/// Returns the specified worksheet range as a 2D array of objects.
let toArray2D (range : Range) = range.Value2 :?> obj [,]

/// Returns the specified worksheet range as a 2D array of objects, together with a 1-based
/// row-index and column-index for each cell.
let toArray2Drc (range : Range) = 
    range
    |> toArray2D
    |> Array2D.mapi (fun i j o -> (i, j, o))

/// Takes a function and an Excel range, and returns the results of applying the function to each individual cell.
let map (f : obj -> 'T) (range : Range) = 
    range
    |> toArray2D
    |> Array2D.map f

/// Takes a function and an Excel range, and returns the results of applying the function to each individual cell.
let maprc (f : int -> int -> obj -> 'T) (range : Range) = 
    range
    |> toArray2Drc
    |> Array2D.map (fun (r, c, o) -> f r c o)

/// Takes a function and an Excel range, and applies the function to each individual cell.
let iter (f : obj -> (Range -> unit) option) (range : Range) = 
    let fc = range |> map f
    for r = 1 to range.Rows.Count do
        for c = 1 to range.Columns.Count do
            let fcrw = fc.[r, c]
            if fcrw.IsSome then 
                let cell = range.[r, c]
                fcrw.Value cell
                cell.Dispose() //very important!

/// Takes a function and an Excel range, and applies the function to each individual cell,
/// providing 1-based row-index and column-index for each cell as arguments to the function.
let iterrc (f : int -> int -> obj -> (Range -> unit) option) (range : Range) = 
    let fc = range |> maprc f
    for r = 1 to range.Rows.Count do
        for c = 1 to range.Columns.Count do
            let fcrw = fc.[r, c]
            if fcrw.IsSome then 
                let cell = range.[r, c]
                fcrw.Value cell
                cell.Dispose() //very important!

///// Examples /////
//open Excel first before running!
// use active workbook:
let xlapp = Application.GetActiveInstance(true)
let wb = xlapp.ActiveWorkbook
// Get a reference to the workbook:
let sh = wb.Sheets.["Sheet1"] :?> Worksheet
// Get a reference to a named range:
let exampleRange = sh.Range(sh.Cells.[1, 1], sh.Cells.[300, 50])
// populate
let vals = Array2D.init 300 50 (fun i j -> i * j)

exampleRange.Value2 <- vals

// toArray2D example:
let cellCount = 
    let arr = exampleRange |> toArray2D
    Array2D.length1 arr * Array2D.length2 arr

// toArray2Drc example:
let listCellRC = 
    exampleRange
    |> toArray2Drc
    |> Array2D.iter (fun (r, c, o) -> printfn "row:%i col:%i cell:%s" r c (o.ToString()))

// map example:
let floatTotal = 
    exampleRange
    |> map (fun cell -> cellDouble cell)
    |> Seq.cast<float>
    |> Seq.sum

// maprc example:
let evenTotal = 
    exampleRange
    |> maprc (fun r _ cell -> 
           if r % 2 = 0 then cellDouble cell
           else 0.0)
    |> Seq.cast<float>
    |> Seq.sum

// iter example
let highlightRange = exampleRange |> iter (fun o -> Some(fun cell -> cell.Interior.Color <- 65535)) // Yellow
// Entire range is yellow

// iterrc example
let chequerRange = 
    exampleRange |> iterrc (fun r c cell -> 
                        if (r % 2 = 0) && (c % 2 <> 0) || (r % 2 <> 0) && (c % 2 = 0) then Some(fun cell -> cell.Interior.Color <- 65535) // Yellow
                        else Some(fun cell -> cell.Interior.Color <- 255)) // Red
// Range is fetchingly chequered in red and yellow

// filtered example:
let colourOddInts = 
    let oddIntRange o = 
        let cellVal = cellDouble o
        ((int cellVal) % 2) <> 0
    exampleRange |> iter (fun o -> 
                        if (oddIntRange o) then Some(fun cell -> cell.Interior.Color <- 65535)
                        else None)
