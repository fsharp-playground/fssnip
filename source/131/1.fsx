module String =

  let split (sep:string) (str:string) =
    match sep, str with
    | ((null | ""), _) | (_, (null | "")) -> seq [str]
    | _ ->
      let n, j = str.Length, sep.Length
      let rec loop p = 
        seq {
          if p < n then
            let i = match str.IndexOf(sep, p) with -1 -> n | i -> i
            yield str.Substring(p, i - p)
            yield! loop (i + j)
        }
      loop 0
