open Microsoft.FSharp.Reflection

let toSepFile sep header (fileName:string) (s:'record seq)=
 
    let schemaType=typeof<'record>
    let fields = Reflection.FSharpType.GetRecordFields(schemaType) 

    let toStr fields = 
        fields
        |> Seq.fold(fun res field-> res+field+sep) ""
    
    use w = new System.IO.StreamWriter(fileName)

    if header then 
        let header_str= fields
                        |> Seq.map(fun field -> field.Name)
                        |> toStr
                    
        w.WriteLine(header_str)

    let elemToStr (elem:'record) = 
        //for each field get value 
        fields
        |> Seq.map(fun field -> string (FSharpValue.GetRecordField(elem,field)))
        |> toStr
   
    s
    |>Seq.map(elemToStr)
    |>Seq.iter(fun elem -> w.WriteLine(elem))
