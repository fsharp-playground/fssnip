// The first version fixed to do use memoize
let memoize (f: 'a -> 'b) =
    let t = new System.Collections.Generic.Dictionary<'a,'b>()
    fun n ->
        let ok, res = t.TryGetValue(n)
        if ok then res
        else let res = f n
             t.Add(n,res)
             res

let fibs n = 
    let rec fibs' = fun n -> match n with 0 -> 1 | 1 -> 1 | _ -> (fibs' (n - 1)) + (fibs' (n - 2))
    memoize(fibs') n


let rec fibSeq n = 
    if fibs n > 4000000 then [] else fibs n :: fibSeq  (n + 1)

printf "sum: %d\n" (fibSeq 1 |>List.filter (fun x -> x % 2 = 0) |> List.sum)

