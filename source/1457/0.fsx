open System
open System.IO

let readValue (reader:BinaryReader) cellIndex = 
    // set stream to correct location
    reader.BaseStream.Seek(int64 (cellIndex*4), SeekOrigin.Begin) |> ignore
    match reader.ReadInt32() with
    | Int32.MinValue -> None
    | v -> Some(v)
        
let readValues indices fileName = 
    use reader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
    // Use list or array to force creation of values (otherwise reader gets disposed before the values are read)
    let values = Array.map (readValue reader) indices
    values