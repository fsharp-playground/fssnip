module String =
    /// Converts a string into a list of characters.
    let explode (s:string) =
        let rec loop n acc =
            if n = 0 then
                acc
            else
                loop (n-1) (s.[n-1] :: acc)
        loop s.Length []

    /// Converts a list of characters into a string.
    let implode (xs:char list) =
        let sb = System.Text.StringBuilder(xs.Length)
        xs |> List.iter (sb.Append >> ignore)
        sb.ToString()
