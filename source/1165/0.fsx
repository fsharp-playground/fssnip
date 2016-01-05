open System
open System.IO

let WriteAllLinesNoFinalLineEnd outFileName (lines : seq<string>) =
    let count = lines |> Seq.length
    use writer = new StreamWriter(outFileName, false)
    lines
    |> Seq.iteri (fun i line -> if i < count - 1 then
                                    writer.WriteLine(line)
                                else
                                    writer.Write(line))