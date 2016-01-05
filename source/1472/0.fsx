open NodaTime
open FsCheck

type NodaGen =
    static member Instant () =
        Arb.generate<System.DateTime>
        |> Gen.map (fun dt -> dt.ToUniversalTime())
        |> Gen.map (fun dt -> Instant.FromDateTimeUtc dt)
        |> Arb.fromGen

// If using NUnit...
// [<SetUp>]
let setup () =
    do Arb.register<NodaGen>() |> ignore        
     
