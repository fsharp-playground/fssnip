module php =
  let deserialise (str : string) = 
    let gs (s : string) (k : int) = 
      try
      let b = s.IndexOf(":", k) + 1
      let e = s.IndexOf(":", b)
      let l = System.Int32.Parse(s.Substring(b, e - b))
      s.Substring(e + 2, l)

      with | ex -> ""
      
    let sn i = str.[i - 1]
    
    let ex (k,s) = 
     
      match (k,s) with
      | (_,':') -> 
        match sn k with
        | 's' -> gs str k
        | _ -> ""
      | (_,'N') -> 
        match sn k with
        | ';' -> "No value"
        |  _  -> ""
      | _ -> ""
      
    seq { 
        for i=0 to str.Length - 1 do yield i, str.[i] 
    }   |> Seq.map ex
        |> Seq.filter ((<>) "")
