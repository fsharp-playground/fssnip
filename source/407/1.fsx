let split (selector:'a->bool) (source:seq<'a>) :seq<seq<'a>>=
  let i = ref 0
  source
  |> Seq.groupBy (fun elem -> if selector elem then incr i
                              !i)
  |> Seq.map snd