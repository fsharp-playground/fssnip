#r "System.Xml.dll";;
open System;;
open System.Linq;;
open System.Xml;;
open System.Xml.Schema;;
 
// open sample XML files, and infer a schema
let samples = [@"file1.xml"; @"file2.xml"; @"fileN.xml" ];;
let inference = new XmlSchemaInference();;
let inferred =
    samples
    |> List.map (fun x -> XmlReader.Create x)
    |> List.fold (fun schema x -> inference.InferSchema(x, schema)) (new XmlSchemaSet());;

Enumerable.Cast<XmlSchema>(inferred.Schemas())
|> Seq.iter (fun schema -> schema.Write(Console.Out));;