module LazyString = 
    type LazyString = char seq 

    let fromString (s:string) : LazyString = seq { for i in s do yield i }
    let toString (l:LazyString) = new string(Seq.toArray l)
    let concat (a:LazyString) (b:LazyString) : LazyString = Seq.concat [b;a]
    
    let replace (what:LazyString) (from:LazyString) (src:LazyString) : LazyString = 
        seq {
            for i in (src |> toString).Replace(what |> toString, from |> toString) do
                yield i
        }
    
    let substr (startIndex:int) (src:LazyString) : LazyString = 
        src |> Seq.skip (startIndex+1)

    let substring (startIndex:int) (count:int) (src:LazyString) : LazyString = 
        src |> Seq.skip (startIndex+1) |> Seq.take count

    //TODO: Implement other standard string operations

LazyString.fromString "Hello "
|> LazyString.concat (LazyString.fromString "C# world")
|> LazyString.replace (LazyString.fromString "C#") (LazyString.fromString "F#")
|> LazyString.toString
|> printfn "%s"