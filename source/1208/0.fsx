type OnewaySequence<'a>(input) =
    let enumerable : IEnumerator<'a> = input
    member x.read() = 
        let ret = enumerable
        ignore input.MoveNext
        ret
let exponentialSequence parameter seed chunk = 
    let random = new MathNet.Numerics.Random.MersenneTwister(seed, false)
    let seq = seq { while true do yield! Array.map (fun x -> 1.0 - exp(- parameter * x) ) (random.NextDouble(chunk)) }
    OnewaySequence(seq.GetEnumerator())