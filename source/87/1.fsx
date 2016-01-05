//generic memoize function from http://blogs.msdn.com/b/dsyme/archive/2007/05/31/a-sample-of-the-memoization-pattern-in-f.aspx
let memoize (f: 'a -> 'b) =
    let t = new System.Collections.Generic.Dictionary<'a,'b>()
    fun n ->
        let ok, res = t.TryGetValue(n)
        if ok then res
        else let res = f n
             t.Add(n,res)
             res

let rec fibs n = 
    let fibs' = fun n -> match n with 0 -> 1 | 1 -> 1 | _ -> (fibs (n - 1)) + (fibs (n - 2))
    memoize(fibs') n

let rec fibSeq n = 
    if fibs n > 4000000 then [] else fibs n :: fibSeq  (n + 1)

printf "sum: %d\n" (fibSeq 1 |>List.filter (fun x -> x % 2 = 0) |> List.sum)


(*alternative version *)
let rec fibonacci m n =
  match n with
  | x when x>= 4000000 -> 0 
  | x when x%2=0 -> n + fibonacci n (n+m)
  | x -> 0 + fibonacci n (n+m)

let v = fibonacci 0 1 

printfn "%d" v

