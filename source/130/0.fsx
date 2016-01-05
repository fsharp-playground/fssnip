open System
open System.Text

[<CompiledName("QuoteIdentifier")>]
let quoteIdentifier id quotePrefix quoteSuffix = 
    let isEmpty = String.IsNullOrEmpty
    let notEmpty = not << isEmpty
    let prefix, suffix = quotePrefix, quoteSuffix
    let equal strA indexA strB = (String.CompareOrdinal(strA, indexA, strB, 0, strB.Length) = 0)
    let getNext start =
        let builder = new StringBuilder()
        let append (s:string) = builder.Append(s) |> ignore
        let quoted = 
            if notEmpty prefix then
                append prefix
                equal id start prefix
            else false
        let index = if quoted then start + prefix.Length else start
        let rec loop i n =
            if i = id.Length then (i, n)
            else
                if notEmpty suffix && equal id i suffix then
                    if (i + suffix.Length) = id.Length then (i, id.Length)
                    elif id.[i + suffix.Length] = '.' then (i, i + suffix.Length + 1)
                    else
                        append suffix
                        append suffix
                        loop (if (equal id (i + suffix.Length) suffix) then i + 2 else i + 1) n
                else
                    if not quoted && id.[i] = '.' then (i, i + 1)
                    else
                        append (id.[i].ToString())
                        loop (i + 1) (i + 1)
        let _, next = loop index index
        if notEmpty suffix then append suffix
        (builder.ToString(), next)
    let split() = 
        0 
        |> Seq.unfold (function 
            | i when i = id.Length -> None 
            | i -> Some (getNext i)) 
        |> Seq.toArray
    if isEmpty id then id
    else String.Join(".", split())

//Usage:
let quote id = quoteIdentifier id "[" "]"
quote "dbo.MyTable" //Output: "[dbo].[MyTable]"
quote "dbo.My[Table" //Output: "[dbo].[My[Table]"
quote "dbo.My]Table" //Output: "[dbo].[My]]Table]"
quote "dbo.[MyTable]" //Output: "[dbo].[MyTable]"