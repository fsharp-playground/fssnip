open NUnit.Framework
open Xunit
open Xunit.Extensions
open FsUnit

let PresentValueOf amt time rate =
    amt * System.Math.Pow(1.0 + rate, -time)

[<TestFixture>]
type ``Given a future amount f at time t at rate i``() =

    [<Theory>]
    [<InlineData(0.0, 0.0, 0.0, 0.0)>]
    [<InlineData(0.0, 0.0, 0.1, 0.0)>]
    [<InlineData(0.0, 1.0, 0.0, 0.0)>]
    [<InlineData(1.0, 0.0, 0.0, 1.0)>]
    [<InlineData(1.0, 0.0, 0.1, 1.0)>]
    [<InlineData(1.1, 1.0, 0.1, 1.0)>]
    [<InlineData(1.2, 1.0, 0.2, 1.0)>]
    [<InlineData(1.21, 2.0, 0.1, 1.0)>]
    member x.``the present value is correct``(f, t, i, expected) =
        let actual = PresentValueOf f t i
        actual |> should (equalWithin 1.0e-9) expected 
