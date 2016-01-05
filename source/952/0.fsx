// F# uses object-oriented 'reference equality' when 
// comparing types defined as classes (but functional
// style structural equality for ADTs)
type Name() = 
  class end

// So we can use '=' to test whether two names are equal
let eq n1 n2 = n1 = n2

let n1 = Name()
let n2 = Name()
eq n1 n1 // true
eq n1 n2 // false


// Function that works on any F# (and .NET) type can be
// implemented using reflection mechanism (which is quite
// powerful, but it is based on .NET so it is not very
// elegant and also not very efficient)
open Microsoft.FSharp.Reflection

// A function that replaces all occurrences of 'n1' in 'any' with 'n2'
let rec sw<'a> (n1:Name) (n2:Name) (any:'a) : 'a =
  // Get the run-time representation of the type of 'any'
  let typ = any.GetType()
  // If it is a Name, then check if it equals 'n1' and substitute
  if typ = typeof<Name> then
    // We know that it is 'Name', but we need to convert the type
    // from 'a to Name (using 'unbox') and then back using 'box'
    // and downcast operator (:?>)
    let asName = unbox<Name> any
    if asName = n1 then (box n2) :?> 'a else any
  // If it is a record, walk over all fields recursively
  elif FSharpType.IsRecord(typ) then
    let fields = FSharpValue.GetRecordFields(any)
    let fields' = fields |> Array.map (sw<_> n1 n2) 
    FSharpValue.MakeRecord(typ, fields') :?> 'a
  // Dtto for unions and tuples
  elif FSharpType.IsUnion(typ) then
    let case, fields = FSharpValue.GetUnionFields(any, typ)
    let fields' = fields |> Array.map (sw<_> n1 n2)
    FSharpValue.MakeUnion(case, fields') :?> 'a
  elif FSharpType.IsTuple(typ) then
    let fields = FSharpValue.GetTupleFields(any)
    let fields' = fields |> Array.map (sw<_> n1 n2)
    FSharpValue.MakeTuple(fields', typ) :?> 'a
  // Not sure what to do when the value is a function (?)
  elif FSharpType.IsFunction(typ) then
    failwith "Function values are not supported"
  // If it is a primitive type, just return the original
  elif typ.IsValueType || typ = typeof<string> then 
    any
  // This also does not support classes (and types defined
  // in .NET libraries), which would make it uglier
  else
    failwithf ".NET types are not supported %A" typ.Name


// Sample representation of expressions using F# record & union
type BinaryInfo = 
  { Op : string
    Left : Expr 
    Right : Expr }

and Expr =
  | Var of Name
  | Binary of BinaryInfo

// If we replace 'n1' with 'n2' in 'sample1' then we
// should get a value that will be equal to 'sample2'
let sample1 =
  (Binary { Op = "+"; Left = Var n1; Right = Var n1 })
let sample2 =
  (Binary { Op = "+"; Left = Var n2; Right = Var n2 })

sw n1 n2 sample1 = sample1 // false 
sw n1 n2 sample1 = sample2 // true