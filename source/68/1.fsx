open System
 
let rec scramble (sqn : seq<'T>) = 
    let rnd = new Random()
    /// Removes an element from a sequence.
    let remove n sqn = sqn |> Seq.filter (fun x -> x <> n)
 
    seq {
        let x = sqn |> Seq.nth (rnd.Next(0, sqn |> Seq.length))
        yield x
        let sqn' = remove x sqn
        if not (sqn' |> Seq.isEmpty) then
            yield! scramble sqn'
    }
 
// Example:
let test = scramble ['1' .. '9'] |> Seq.toList
// Output:
// val test : char list = ['3'; '6'; '7'; '5'; '4'; '8'; '2'; '1'; '9']