// String Interpolation
// http://fslang.uservoice.com/forums/245727-f-language/suggestions/5678806-string-interpolation 
let (+@) a b = sprintf "%s%A" a b

let a = 40
let p = "John"
let b = " " + p + " has " +@ a*2 + " items"
printfn "%s" b


// Add Option.filter
// http://fslang.uservoice.com/forums/245727-f-language/suggestions/5674917-add-option-filter
module Option =
  let filter cond = 
    Option.bind(fun x -> if cond x then Some x else None)

let x1 = Some 1 
let x2 = Some 3 
let y1 = x1 |> Option.filter (fun i -> i = 3) // Returns None 
let y2 = x2 |> Option.filter (fun i -> i = 3) // Returns Some 3


// Allow the use of the tuple operator (,) as a function.
// http://fslang.uservoice.com/forums/245727-f-language/suggestions/5669324-allow-the-use-of-the-tuple-operator-as-a-funct 
let ``(,)`` x y = (x,y)


// F# 3.0 query expression with pipelined style
// http://fslang.uservoice.com/forums/245727-f-language/suggestions/5666371-f-3-0-query-expression-with-pipelined-style
open System.Linq
type Row = { ColumnA:int }
let source = [ for i in 0..9 -> { ColumnA = i } ]
source
  .OrderBy(fun a -> a.ColumnA)
  .Skip(10)
  .Take(20)
|> Seq.iter (fun a -> printfn "%s" (string a.ColumnA))


// Add syntactic sugar for functions ala Scala/Clojure
// http://fslang.uservoice.com/forums/245727-f-language/suggestions/5665355-add-syntactic-sugar-for-functions-ala-scala-clojur
type PlaceHolder () =
  static member (+) (__:PlaceHolder, right) = fun left -> left + right
let __ = PlaceHolder()
"Place" |> __ + "Holder" |> printfn "%s"


// Allow Pattern Matching on Types 
// http://fslang.uservoice.com/forums/245727-f-language/suggestions/5664335-allow-pattern-matching-on-types
open System
match typeof<float> |> Activator.CreateInstance with 
| :? int -> printfn "int!"
| :? float -> printfn "float!"
| _ -> printfn "didn't match!"


// Allow private constructors on DU cases 
// http://fslang.uservoice.com/forums/245727-f-language/suggestions/5663374-allow-private-constructors-on-du-cases
// ADDED: Mr @iceypoi kindly gived a better version of private DU constructor!
// URL: http://fssnip.net/ma
type EmailAddress = 
  | ValidEmail of string 
  | InvalidEmail of string 

[<AutoOpen>]
module Hide =
  type ``Don't use this! It's private constructor for the DU`` () = do ()
  module EmailAddress = 
    let ValidEmail = ``Don't use this! It's private constructor for the DU`` ()


// Allow custom equality on record types.
// http://fslang.uservoice.com/forums/245727-f-language/suggestions/5663332-allow-custom-equality-on-record-types
[<CustomEquality; CustomComparison>]
type Record =   
  { Field:string }
  override __.Equals (o:obj) = false
  override __.GetHashCode () = 0
  interface System.Collections.IStructuralComparable with
    member __.CompareTo (o:obj, _) = 0
