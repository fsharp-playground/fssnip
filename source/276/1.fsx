let subs (s : string) =
   match s.Length with
   | 0 -> Seq.empty
   | 1 -> Seq.singleton [|s.[0]|]
   | _ ->
      let sub' n = Seq.windowed n s
      Seq.unfold (fun n -> if n <= s.Length then Some(sub' n, n+1) else None) 2
      |> Seq.collect id

let isPalindrome (s : 'a[]) =
   let l = s.Length-1
   seq { 0..(l+1)/2 } |> Seq.forall (fun i -> s.[i] = s.[l-i])

let palindromes s =
   s
   |> subs
   |> Seq.filter isPalindrome
   |> Seq.map (fun x -> System.String x)