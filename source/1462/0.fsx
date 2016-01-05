open System.IO
open System.CodeDom
open System.CodeDom.Compiler

let s = """\<a\s.+?href=("|')(?<link>.+?)\1"""
let primitive = new CodePrimitiveExpression(s)
let provider = CodeDomProvider.CreateProvider("CSharp")
let options = new CodeGeneratorOptions()
let writer = new StringWriter ()
try
    try
        provider.GenerateCodeFromExpression(primitive, writer, options)
        writer.Flush()
        writer.GetStringBuilder().ToString()
        |> printfn "%s"
    with
        | exn -> printfn "%A" exn
finally
    writer.Close()