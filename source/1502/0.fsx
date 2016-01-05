open System.IO

let temp = ResizeArray()
[ for line in File.ReadLines "RELEASE_NOTES.md"  do
    if line.StartsWith("#") then
        yield List.ofSeq temp
        temp.Clear()
    temp.Add line
  yield List.ofSeq temp ]
|> List.rev
|> List.collect id
|> fun lines -> File.WriteAllLines("RELEASE_NOTES.md", lines)