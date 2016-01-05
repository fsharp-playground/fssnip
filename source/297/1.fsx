// [snippet:Implementation]
type TempFile() =
    let path = System.IO.Path.GetTempFileName()
    member x.Path = path
    interface System.IDisposable with
        member x.Dispose() = System.IO.File.Delete(path)
// [/snippet]

// Mock types and declarations for the example below
type Assert = 
  static member Equal(a:obj, b:obj) = true

type FactAttribute() = 
  inherit System.Attribute()

let filewrite path data = ()
let fileread path = ""

module Tests=
// [snippet:Sample usage]
    [<Fact>] //check that a round trip yields the same result
    let ReadWriteRoundTrip() =
        use tmp = new TempFile()
        let data = "this is a string\nOn two lines\t."
        filewrite tmp.Path data
        Assert.Equal(data, fileread tmp.Path)
// [/snippet]