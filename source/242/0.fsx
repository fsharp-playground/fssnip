module Csv

open System.IO
open Microsoft.FSharp.Reflection

type Array =
    static member join delimiter xs = 
        xs 
        |> Array.map (fun x -> x.ToString())
        |> String.concat delimiter

type Seq =
    static member write (path:string) (data:seq<'a>): 'result = 
        use writer = new StreamWriter(path)
        data
        |> Seq.iter writer.WriteLine 

    static member csv (separator:string) (headerMapping:string -> string) option ( data:seq<'a>) =
        seq {
            let dataType = typeof<'a>

            let header = 
                match dataType with
                | ty when FSharpType.IsRecord ty ->
                    FSharpType.GetRecordFields dataType
                    |> Array.map (fun info -> headerMapping info.Name)                    
                | ty when FSharpType.IsTuple ty -> 
                    FSharpType.GetTupleElements dataType
                    |> Array.mapi (fun idx info -> headerMapping(string idx) )
                | _ -> dataType.GetProperties()
                       |> Array.map (fun info -> headerMapping info.Name)

            yield header |> Array.join separator
                                    
            let lines =
                match dataType with 
                | ty when FSharpType.IsRecord ty -> 
                    data |> Seq.map FSharpValue.GetRecordFields
                | ty when FSharpType.IsTuple ty ->
                    data |> Seq.map FSharpValue.GetTupleFields
                | _ -> 
                    let props = dataType.GetProperties()
                    data |> Seq.map ( fun line -> 
                              props |> Array.map ( fun prop ->
                                prop.GetValue(line, null) ))                                     

            yield! lines |> Seq.map (Array.join separator)        
        }
//Example
type Test(colA:string, colB:int) = 
    member x.ColA = colA
    member x.ColB = colB

let testData = seq { for i in 1..10 -> new Test("col"+string(i), i) }

// using all public class properties for serialization
testData
|> Seq.csv "\t" (fun propertyName -> propertyName)
|> Seq.write "test_with_class_properties.csv"

// using a tuple projection
testData
|> Seq.distinctBy (fun testInstance -> testInstance.ColA)  
|> Seq.map (fun probe -> (probe.ColB) )
|> Seq.csv "\t" (fun columnName -> 
                    match columnName with 
                    | "0" -> "ColB"
                    | _ -> columnName)
|> Seq.write "test_with_tuple_projection.csv"