    let internal escape_char (c:char) : bool * string = 
        match c with 
        | '\r' -> true, "\\r"
        | '\n' -> true, "\\n"
        | '\t' -> true, "\\t"
        | '"'  -> true, "\\\""
        | _ -> false, null

    let internal escape_string (str:string) = 
        let buf = new StringBuilder(str.Length)
        for c in str do
            match escape_char c with
            | true, s  -> buf.Append s |> ignore
            | false, _ -> buf.Append c |> ignore
        buf.ToString()
