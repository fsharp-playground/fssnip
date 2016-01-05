let memoize fn =
  let cache = new System.Collections.Generic.Dictionary<_,_>()
  (fun x ->
    match cache.TryGetValue x with
    | true, v -> v
    | false, _ -> let v = fn (x)
                  cache.Add(x,v)
                  v)

let square n = 
  printfn "computing square of %d" n
  n*n

let memSquare = memoize square
memSquare 42
memSquare 42
memSquare 99 


