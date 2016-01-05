module TestFsharp

open Microsoft.VisualStudio.TestTools.UnitTesting

type SampleClassType(argument1: int, argument2: int) = 
    /// Get the sum of the object arguments
    member x.Sum = argument1 + argument2
    /// Create an instance of the class type
    static member Create() = SampleClassType(3, 4)

let t = SampleClassType(5, 5)
let t2 = SampleClassType(6, 6)
let y = t.Sum
let z = 0

[<TestClass>]
type TestCaseUtil() =
    [<TestMethod>]
    [<TestCategory("TestFsharp")>]
    [<Description("Assert Not Equal")>]
    member this.AssertNot() =
        Assert.AreNotEqual(5, y)
    [<TestMethod>]
    [<TestCategory("TestFsharp")>]
    [<Description("Assert Equal")>]
    member this.AssertEqual() =
        Assert.AreEqual(10, y)
//note each TestClass cannot contain the same unique signature more than once
//even if you are testing a method in a different object
[<TestClass>]
type TestCaseUtil2() =
    [<TestMethod>]
    [<TestCategory("TestFsharp")>]
    [<Description("Assert Equal2")>]
    member this.AssertEqual() =
        Assert.AreEqual(12, t2.Sum)