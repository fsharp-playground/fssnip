module FSharp
    module DlrHosting = 
        open System.Reflection
        open Microsoft.Scripting
        open Microsoft.FSharp.Reflection
        open Microsoft.Scripting.Hosting
       
        let mutable dlrEngine:ScriptEngine = null
                
        let (?) (instance : obj) member' : 'Result = 
            let resultType =  typeof<'Result>
            
            // did we get invoked as a callable member ?
            if FSharpType.IsFunction resultType then
                //the called member from the dlr type should really be a callable!
                let callable = dlrEngine.Operations.GetMember(instance, member')
                if not (dlrEngine.Operations.IsCallable(callable))
                    then failwithf "%s is not callable" member'
                
                let invokeWithNoArguments = fun _ -> [||]
                let invokeWithOneArgument = fun arg -> [|arg|]
                let invokeWithManyArguments = fun args -> FSharpValue.GetTupleFields(args) 
                
                let functionType, _ = FSharpType.GetFunctionElements(resultType)
                let determineArgsFromInvocation = 
                    match functionType with
                    | type' when type' = typeof<unit> -> invokeWithNoArguments
                    | type' when FSharpType.IsTuple type' -> invokeWithManyArguments
                    | _ -> invokeWithOneArgument
                               
                downcast FSharpValue.MakeFunction(resultType, fun argsFromInvocation -> 
                    dlrEngine.Operations.Invoke(callable, determineArgsFromInvocation(argsFromInvocation) ) ) 
            else
                downcast dlrEngine.Operations.GetMember(instance, member')
        
        let (?<-) (instance : obj) member' value = 
            dlrEngine.Operations.SetMember(instance, member', value)

        let createScriptSource (scriptText:string) = 
            dlrEngine.CreateScriptSourceFromString(scriptText, SourceCodeKind.AutoDetect)

        let executeScriptSource (source:ScriptSource) (scope:ScriptScope) = 
            source.Execute(scope)

        let (|>>) (scope : ScriptScope) codeString = 
            let source = createScriptSource(codeString)
            executeScriptSource source scope |> ignore
            scope

        let (|>?) (scope : ScriptScope) codeString = 
            let source = createScriptSource(codeString)
            executeScriptSource source scope


//Example:  I've used the template engine Jinja2 for rendering
// HTML for demonstrating interop.
open System.Collections.Generic
open System
open System.IO

open Microsoft.FSharp.Core
open FSharp.DlrHosting
open IronPython.Hosting


dlrEngine <- Python.CreateEngine()

let sitePackages = Path.Combine( Environment.CurrentDirectory, "Lib", "site-packages")
let sys = dlrEngine.GetSysModule()
sys?path?append(sitePackages)

let convertFunc f =
    FSharpFunc.ToConverter f

let pymodule = createScope() |>> @"
from jinja2 import FileSystemLoader, DictLoader, FunctionLoader
from jinja2 import Environment
import os

def render( data_context, templateLoader, template_name ):
    env = Environment(loader=templateLoader)
    print templateLoader
    template = env.get_template(template_name)        
    return template.render( data_context )     
"

let loadTemplate name = 
   match name with 
    | "result.tmpl" -> "<html><body>{{ greeting }}</body></html>"
    | _ -> "" 

let templateLoader = pymodule?FunctionLoader(convertFunc loadTemplate) |> box

let data = new Dictionary<obj,obj>()
data.["greeting"] <- "hello python from fsharp"

let rendered:string = pymodule?render(data, templateLoader, "result.tmpl") 

printfn "%s" rendered
pymodule |>> "test = 2+2" |> ignore
printfn "%i" pymodule?test |> unbox
printfn "%i" (pymodule |>? "3+4" |> unbox)
        
