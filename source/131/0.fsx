        open System

        type System.String with 
            member x.LazySplit(separator:string) =
                if String.IsNullOrEmpty(x) || String.IsNullOrEmpty(separator) then 
                    Seq.singleton x
                else
                    let n, j = x.Length, separator.Length
                    let rec loop p = 
                        seq {
                            if p < n then
                                let i = match x.IndexOf(separator, p) with -1 -> n | i' -> i'
                                yield x.Substring(p, i - p)
                                yield! loop (i + j)
                        }
                    loop 0
