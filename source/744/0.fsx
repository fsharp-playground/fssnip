let tryNth index (source : seq<_>) = 
    let rec tryNth' index (e : System.Collections.Generic.IEnumerator<'a>) = 
        if not (e.MoveNext()) then None
        else if index < 0 then None
        else if index = 0 then Some(e.Current)
        else tryNth' (index-1) e

    use e = source.GetEnumerator()
    tryNth' index e