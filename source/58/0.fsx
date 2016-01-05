type Random(seed: int option) =
  let rndSeed() =
    let t = System.DateTime.Now
    (t.Month + t.Minute) * (t.Millisecond - t.Second) + t.Hour
  let seedN = defaultArg seed (rndSeed())
  
  let random = new System.Random(seedN)

  new () = Random(None)
  new (seed: int) = Random(Some seed)
  
  member x.Seed = seedN

  member x.NextInt () = random.Next()
  member x.NextInt (max: int<'a>) = random.Next(int max) * (max / int max)
  member x.NextInt (min: int<'a>, max: int<'a>) = random.Next(int min, int max) * (max / int max)

  member x.NextFloat () = random.NextDouble()
  member x.NextFloat (max: float<'a>) = random.NextDouble() * max
  member x.NextFloat (min: float<'a>, max: float<'a>) = min + random.NextDouble() * (max - min)

// Usage
let rnd = Random()
printfn "Seed = %d" rnd.Seed
rnd.NextInt() |> printfn "NextInt = %d"
rnd.NextInt(10) |> printfn "NextInt = %d"
rnd.NextInt(-10, 10) |> printfn "NextInt = %d"
rnd.NextFloat() |> printfn "NextFloat = %f"
rnd.NextFloat(10.) |> printfn "NextFloat = %f"
rnd.NextFloat(-10., 10.) |> printfn "NextFloat = %f"