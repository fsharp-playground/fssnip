let name = (query {
      for t in context.``[public].[team]`` do
           where (t.id = 6)
           select (t.name)
           take 1
} |> Array.ofSeq ).[0]