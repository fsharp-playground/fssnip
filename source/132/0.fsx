module Identifier =

    open System

    let changeCase toCamelCase (str : string) =
        if String.IsNullOrEmpty(str) then str
        else
            let isUpper = Char.IsUpper
            let n = str.Length
            let builder = new System.Text.StringBuilder()
            let append (s:string) = builder.Append(s) |> ignore
            let rec loop i j =
                let k = 
                    if i = n || (isUpper str.[i] && (not (isUpper str.[i - 1]) 
                                || ((i + 1) <> n && not (isUpper str.[i + 1])))) 
                        then
                        if j = 0 && toCamelCase then
                            append (str.Substring(j, i - j).ToLower())
                        elif (i - j) > 2 then
                            append (str.Substring(j, 1))
                            append (str.Substring(j + 1, i - j - 1).ToLower())
                        else
                            append (str.Substring(j, i - j))
                        i
                    else
                        j
                if i = n then builder.ToString()
                else loop (i + 1) k
            loop 1 0

    type System.String with
        member x.ToPascalCase() = changeCase false x
        member x.ToCamelCase() = changeCase true x

    printfn "%s" ("HTMLParser".ToPascalCase()) //prints: HtmlParser
    printfn "%s" ("HTMLParser".ToCamelCase()) //prints: htmlParser
