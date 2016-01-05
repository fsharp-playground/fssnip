module ZipProvider
open System.IO
open System.IO.Compression
open Samples.FSharp.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices

type ZipArchiveEntry with
  member arh.Text=
    use rdr=new StreamReader(arh.Open())
    rdr.ReadToEnd()

[<TypeProvider>]
type ZipPr() as this =
    inherit TypeProviderForNamespaces()
    let asm,ns = System.Reflection.Assembly.GetExecutingAssembly(),"ZipProvider"
    let IniTy = ProvidedTypeDefinition(asm, ns, "ZipProv", None)
    do IniTy.DefineStaticParameters(
         [ProvidedStaticParameter("path", typeof<string>)],
          fun tyName [|:? string as path|] ->
           let ty = ProvidedTypeDefinition(asm, ns, tyName, None)
           ProvidedConstructor([ProvidedParameter("path",typeof<string>)],
                               InvokeCode=(fun [path]-> <@@ ZipFile.OpenRead(%%path:string) @@>))
             |>ty.AddMember 
           ZipFile.OpenRead(path).Entries
             |>Seq.map (fun ent->let name=ent.FullName
                                 ProvidedProperty(name,typeof<ZipArchiveEntry>,
                                                     GetterCode=fun [arg] -> <@@ ((%%arg:obj):?>ZipArchive).GetEntry(name) @@>))
             |>Seq.toList|>ty.AddMembers
           ty)
       this.AddNamespace(ns, [IniTy])
[<TypeProviderAssembly>]
do()