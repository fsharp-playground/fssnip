//This relies on the "FSharp.PowerPack.Metadata.dll" from the F# powerpack.
//This code will not run inside F# interactive as is requires a compiled F#  
//assembly to retrieve metadata.

#r "FSharp.PowerPack.Metadata.dll"

    open System
    open System.Diagnostics
    open System.Reflection
    open System.Runtime.Serialization

    open Microsoft.FSharp.Metadata
    open Microsoft.FSharp.Reflection

    module FSharpEntity =
        let fromCurrentSite () = 
            let rec getSite (frames:StackFrame list) =
                match frames with
                | [] -> failwith "Unable to get site!"
                | sf::sfl -> 
                    let mb = sf.GetMethod ()
                    if mb.Name <> ".cctor" &&  FSharpType.IsFunction (mb.DeclaringType) = false then mb.DeclaringType
                    else getSite (sfl)

            let st = new StackTrace (1, true)
            let frames = st.GetFrames () |> Array.toList
            let site = getSite (frames)
            FSharpEntity.FromType (site)

//Usage 
    module Test =
        let getCallingEntity () =
            FSharpEntity.fromCurrentSite ()

        