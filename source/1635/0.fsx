
open System.IO
open System.Text.RegularExpressions

let readlines (path:string) =
    let generator (reader:StreamReader) =
        match reader.ReadLine() with
        | null -> None
        | str -> Some (str, reader)

    let reader = new StreamReader(path)
    Seq.unfold generator reader

let touch path = File.Create(path).Close()

let up path = Path.GetFullPath(Path.Combine(path, ".."))
let upx x path = [1..x] |> List.fold (fun p _ -> up p) path

let mkdir current folder = 
    let path = Path.Combine(current, folder)
    Directory.CreateDirectory(path) |> ignore
    path

let parse line =
    let re = new Regex(@"^(?<blanks>\|\s*)*((?<kind>\\|\+)---)?\s*(?<path>.*)$")
    let m = re.Match(line)
    if m.Success then
        let count = m.Groups.["blanks"].Captures.Count
        let kind = m.Groups.["kind"].Value
        let path = m.Groups.["path"].Value
        Some (count, kind, path)
    else None

let (|Out|File|Folder|) line =
    match parse line with
    | Some (count, "", "") -> Out count
    | Some (count, "\\", path) -> Folder (count + 1, path)
    | Some (count, "+", path) -> Folder (count, path)
    | Some (count, "", path) -> File path
    | _ -> failwith <| sprintf "error parsing line %s" line

let traverse path lines =
    let folder (depth,path) line =
        match line with
        | Out count -> (count, upx (depth - count) path)
        | File file -> 
            touch <| Path.Combine(path, file)
            (depth, path)
        | Folder (d, folder) -> (d + 1, mkdir path folder)

    lines
    |> Seq.fold folder (0, path)
    |> ignore

let doit tree target =
    readlines tree
    |> Seq.skip 3
    |> traverse target