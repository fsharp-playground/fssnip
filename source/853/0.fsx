open NUnit.Framework

let shouldMatch (f : 'a -> bool) (x : 'a) =
    if f x then () 
    else raise <| new AssertionException(sprintf "Unexpected result: %A." x)


[<TestFixture>]
type Test() =

    // concoct some random test scenario
    [<Test>]
    member __.``test scenario``() =
        [1..10]
        |> List.map (fun i -> (i,i % 2 = 0))
        |> List.filter snd
        |> shouldMatch (function (_,true) :: _ -> true | _ -> false)
        