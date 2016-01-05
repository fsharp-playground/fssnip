//From NuGet: Sparc.TagCloud
#if INTERACTIVE
#r @"..\packages\Sparc.TagCloud.0.0.1\lib\net40\Sparc.TagCloud.dll"
#else
module MyTagCloud
#endif
    open System.IO
    open Sparc.TagCloud

    let analyzer = new TagCloudAnalyzer()
    let path = @"C:\sourcecodes\"
    let extension = "*.cs" //e.g. c# files
    let lines = 
        Directory.GetFiles(path, extension, SearchOption.AllDirectories)
        |> Seq.map(fun i -> File.ReadAllLines(i)) 
        |> Seq.concat

    let ``analyze and print`` = 
        analyzer.ComputeTagCloud(lines)//.Shuffle()
        |> Seq.where(fun i -> i.Text.Length > 3) //over 3 letter words only...
        |> Seq.take(50) //top 50 only...
        |> Seq.iter(fun r -> printfn "%s\t%i" r.Text r.Count)
