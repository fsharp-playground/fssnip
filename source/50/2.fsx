// You need NUnit (http://nunit.org/)
#r "nunit.framework.dll"
open NUnit.Framework

let inline (==) (actual:#obj) (expected:#obj) = Assert.AreEqual(expected, actual)
let inline (!=) (actual:#obj) (expected:#obj) = Assert.AreNotEqual(expected, actual)
let inline (<->) (actual:#obj) expected = Assert.IsInstanceOf(expected, actual)
let inline (<!>) (actual:#obj) expected = Assert.IsNotInstanceOf(expected, actual)
let ``is null`` anObject = Assert.IsNull(anObject)
let ``is not null`` anObject = Assert.NotNull(anObject)

[<Test>]
let ``1 + 1 = 2``() = 1 + 1 == 2

[<Test>]
let ``1 + 1 + 1 <> 2``() = 1 + 1 + 1 != 2

[<Test>]
let ``"Howdy"B is a byte[]``() = "Howdy"B <-> typeof<byte[]>

[<Test>]
let ``"Howdy" is not a byte[]``() = "Howdy" <!> typeof<byte[]>

[<Test>]
let ``null is null``() = null |> ``is null``

[<Test>]
let ``new obj() is not null``() = let o = obj() in o |> ``is not null``