//Ini Type provider
//type test=IniProv<PathDefaultIniFile:string,?EncodingName:string>
//(default to Encoding.Default)
//IniProv<...>() - default schema
//IniProv<...>(path:string)load ini from path

//example
//open IniProvider
//let z=new IniProv<"c:\test.ini","utf-8">("c:\test.ini")
//z.NextSection.xkey|>printf "%s"

module IniProvider
open System.IO
open System.Text
open System.Text.RegularExpressions
open System.Reflection
open Samples.FSharp.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices

let (|Match|_|) pt str=
  Regex.Match(str,pt)|>function
    |m when m.Success->seq{for g in m.Groups->g.Value}|>Seq.toArray|>Some
    |_->None
let (|Section|PropVal|Coments|)=function
  |Match "\[(.*)\]" x->Section x.[1]
  |Match "(.*?)=(.*)" x->PropVal(x.[1],x.[2])
  |x->Coments x
let DelComents=function
  |Match "(.*?);" x->x.[1]
  |x->x
let ReadIniData path (enc:string)=
  File.ReadLines(path,Encoding.GetEncoding(enc))|>Seq.map DelComents
   |>Seq.fold (fun (a,name,props) x->x|>function
                                      |Section x->(name,props|>List.rev)::a,x,[]
                                      |PropVal propval->a,name,propval::props
                                      |Coments _->a,name,props)
              ([],"",[])|>fun (a,name,props)->(name,props|>List.rev)::a|>List.rev|>List.tail

[<TypeProvider>]
type Ini() as this =
    inherit TypeProviderForNamespaces()
    let asm,ns = System.Reflection.Assembly.GetExecutingAssembly(),"IniProvider"
    let IniTy = ProvidedTypeDefinition(asm, ns, "IniProv", Some(typeof<obj>))
    do IniTy.DefineStaticParameters([ProvidedStaticParameter("path", typeof<string>);
                                     ProvidedStaticParameter("encoding", typeof<string>,Encoding.Default.BodyName)],
                                    fun tyName [|:? string as path; :? string as enc|] ->
                                         let ty = ProvidedTypeDefinition(asm, ns, tyName, Some(typeof<obj>))
                                         let cte=ProvidedConstructor([],InvokeCode=(fun args -> <@@ ReadIniData path enc @@>))
                                         let ctl=ProvidedConstructor([ProvidedParameter("file",typeof<string>)],
                                                                     InvokeCode=(fun [file]-> <@@ ReadIniData %%file enc @@>))
                                         ty.AddMembers [cte;ctl]
                                         let data=ReadIniData path enc
                                         data|>Seq.iter (fun (name,grp)->
                                           let sty=ProvidedTypeDefinition(name,Some(typeof<obj>))
                                           ty.AddMember sty
                                           grp|>Seq.iter (fun (p,v) ->let prop=ProvidedProperty(p,typeof<string>,
                                                                                               GetterCode=fun [arg]-> <@@ ((%%arg:obj):?>(string*string) list)|>Seq.find (fun (a,b)->a=p)|>snd @@>)
                                                                      sty.AddMember prop)
                                           let prop=ProvidedProperty(name,sty,GetterCode=fun [arg]-> <@@ ((%%arg:obj):?>(string*(string*string) list) list)|>Seq.find (fun (a,_)->a=name)|>snd @@>)
                                           ty.AddMember prop)
                                         ty)
                                         
       this.AddNamespace(ns, [IniTy])
[<TypeProviderAssembly>]
do()