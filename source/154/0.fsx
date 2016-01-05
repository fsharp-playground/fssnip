type Euler1Builder() =
    member b.Combine(x, y) = x + y
    member b.Zero() = 0
    member b.Yield(x) = if x % 5 = 0 || x % 3 = 0 then x else 0
    member b.For(vals, f) = vals |> Seq.fold (fun s n -> b.Combine(s, f n)) (b.Zero()) 

let eb = new Euler1Builder()

let pe1_eb limit = eb { for x = 0 to limit - 1 do yield x }