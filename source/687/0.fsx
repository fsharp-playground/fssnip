open System.IO
open MbUnit.Framework

// DSL to define test suites and cases

let inline (->>) name tests =
    let suite = TestSuite(name)
    Seq.iter suite.Children.Add tests
    suite :> Test

let inline (-->) name (test: unit -> unit) =
    TestCase(name, Gallio.Common.Action test) :> Test

// Higher-order functions as setup/teardown

let withMemoryStream f () =
    use ms = new MemoryStream()
    f ms

let withTempFile f () =
    let file = Path.GetTempFileName()
    try 
        f file
    finally
        File.Delete file

// These can be easily composed:

let withMemoryStreamAndTempFile f =
    withMemoryStream (fun s -> withTempFile (fun t -> f s t) <| ())

// Test definition:

let tests = 
    [
        "A test suite" ->> [
            "2 + 2 = 4" --> 
                fun _ -> Assert.AreEqual(4, 2+2)
            "A test subsuite" ->> [
                // generate parameterized tests
                for i in [0..3] do
                for j in [1..4] do
                for k in [3..10] ->
                    sprintf "%d + %d = %d" i j k --> 
                        fun _ -> Assert.AreEqual(k, i+j)
            ]
        ]
        "Another suite" ->> [
            "File exists" -->
                withTempFile (Assert.IsTrue << File.Exists)
            "Write 'Hello World!'" -->
                withMemoryStreamAndTempFile
                    (fun ms tf ->
                        use w = new StreamWriter(ms)
                        w.Write "Hello World!"
                        w.Flush()
                        ms.Position <- 0L
                        File.WriteAllBytes(tf, ms.ToArray())
                        Assert.AreEqual(12L, FileInfo(tf).Length))
        ]
    ]

// boilerplate, MbUnit doesn't allow StaticTestFactory on F# functions yet
type Tests() =
    [<StaticTestFactory>]
    static member factory() = tests