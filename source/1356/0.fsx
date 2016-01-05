//Example
//open FileProvider
//let f=new FileProv<"c:/windows">("c:/windows")
//f.``win.ini``.Text

//new FileProv<complie_directory>(real_directory) // () - to current directory
//FileProv.members_from_file_names...
  //.Text - Get text from file. Encoding=UTF8
  //.GetText(encoding) - Get text from file. Encoding=encoding
  //.StreamR - Stream for read
  //.StreamW - Stream for write
  //.Name - file name
  //.FullName - full file name

//add to project ProvidedTypes-0.2.fs & ProvidedTypes-0.2.fsi from
//http://fsharp3sample.codeplex.com/SourceControl/latest#SampleProviders/Shared/ProvidedTypes-0.2.fs

module FileProvider
open System.IO
open System.Text
open Samples.FSharp.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices

[<TypeProvider>]
type FilePr() as this =
    inherit TypeProviderForNamespaces()
    let asm,ns = System.Reflection.Assembly.GetExecutingAssembly(),"FileProvider"
    let IniTy = ProvidedTypeDefinition(asm, ns, "FileProv", None)
    do IniTy.DefineStaticParameters([ProvidedStaticParameter("path", typeof<string>)],
                                    fun tyName [|:? string as path|] ->
                                         let ty = ProvidedTypeDefinition(asm, ns, tyName, None)
                                         [ProvidedConstructor([ProvidedParameter("path",typeof<string>)],
                                                              InvokeCode=(fun [path]-> <@@ %%path:string @@>))
                                          ProvidedConstructor([],InvokeCode=(fun _ -> <@@ Directory.GetCurrentDirectory() @@>))]
                                           |>ty.AddMembers 
                                         Directory.GetFiles(path)|>Seq.map (fun name->FileInfo(name).Name)
                                          |>Seq.iter (fun (name)->
                                                       let sty=ProvidedTypeDefinition(name,None)
                                                       ty.AddMember sty
                                                       [ProvidedProperty("Text",typeof<string>,
                                                                         GetterCode=fun [path] -> <@@ Path.Combine((%%path:obj):?>string,name)|>File.ReadAllText @@>)
                                                        ProvidedProperty("StreamR",typeof<Stream>,
                                                                         GetterCode=fun [path] -> <@@ Path.Combine((%%path:obj):?>string,name)|>File.OpenRead @@>)
                                                        ProvidedProperty("StreamW",typeof<Stream>,
                                                                         GetterCode=fun [path] -> <@@ Path.Combine((%%path:obj):?>string,name)|>File.OpenWrite @@>)
                                                        ProvidedProperty("Name",typeof<string>,
                                                                         GetterCode=fun _ -> <@@ name @@>)
                                                        ProvidedProperty("FullName",typeof<string>,
                                                                         GetterCode=fun [path] -> <@@ Path.Combine((%%path:obj):?>string,name) @@>)]
                                                         |>sty.AddMembers
                                                       ProvidedMethod("GetText",[ProvidedParameter("Encode",typeof<Encoding>)],typeof<string>,
                                                                      InvokeCode=fun [path;enc] -> <@@ File.ReadAllText( Path.Combine((%%path:obj):?>string,name),(%%enc:>Encoding)) @@>)
                                                         |>sty.AddMember
                                                       let prop=ProvidedProperty(name,sty,GetterCode=fun [arg] -> <@@ (%%arg:obj):?>string @@>)
                                                       ty.AddMember prop)
                                         ty)
       this.AddNamespace(ns, [IniTy])
[<TypeProviderAssembly>]
do()