// given a list of records as input, 
// generates text of the form:
// +--------+--------+--------+
// | Label1 | Label2 | Label3 |
// +--------+--------+--------+
// | Value1 | Value2 | Value3 |
// +--------+--------+--------+
// | Value1'| Value2'| Value3'|
// +--------+--------+--------+


open System
open System.Text

type UntypedRecord = (string * obj) list // label * value list

let prettyPrintTable (f : 'Record -> UntypedRecord) (template : 'Record) (table : 'Record list) =
    // the template argument acts as a means to extract all labels, even if the table empty. Any non-null value should do
    let labels = f template |> List.map fst
    let header = labels |> List.map (fun h -> h, h :> obj)
    let untypedTable = List.map f table

    let rec traverseEntryLengths (map : Map<string,int>) (line : UntypedRecord) =
        match line with
        | [] -> map
        | (label, value) :: rest ->
            let currentLength = defaultArg (map.TryFind label) 0
            let map' = map.Add (label, max currentLength <| value.ToString().Length + 2)
            traverseEntryLengths map' rest

    let lengthMap = List.fold traverseEntryLengths Map.empty (header :: untypedTable)

    let printRecord (record : UntypedRecord) =
        let printEntry (label,value) = //   value   |
            let field = value.ToString()
            let whites = lengthMap.[label] - field.Length
            let gapL = 1
            let gapR = whites - gapL
            String(' ',gapL) + field + String(' ',gapR) + "|"

        List.fold (fun str entry -> str + printEntry entry) "|" record

    let separator = 
        let printColSep label = // ---------+
            String('-', lengthMap.[label]) + "+"

        List.fold (fun str label -> str + printColSep label) "+" labels 

    let builder = new StringBuilder()
    let append txt = builder.AppendLine txt |> ignore

    do
        append separator
        append <| printRecord header
        append separator

        for record in untypedTable do
            append <| printRecord record
            append separator

    builder.ToString()

//
// Example
//

[<Measure>]
type cm

type Person = { Name : string ; Age : int ; Height : int<cm> }

let f (p : Person) = [ ("Name", p.Name :> obj) ; ("Age", p.Age :> obj) ; ("Height (cm)", p.Height :> obj) ]

let print = prettyPrintTable f { Name = "" ; Age = 0 ; Height = 0<cm> }

let people =
    [
        { Name = "Nick" ; Age = 32 ; Height = 175<cm> }
        { Name = "Eirik" ; Age = 27; Height = 175<cm> }
        { Name = "George" ; Age = 35 ; Height = 200<cm> }
    ]

people
|> List.sortBy (fun p -> - p.Age)
|> print
|> printf "%s"