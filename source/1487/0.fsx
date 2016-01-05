open System
open System.IO
open System.Text.RegularExpressions

let inline sanitizePath (replaceWith: string) (path: string) =
    let invFile = Path.GetInvalidFileNameChars ()
    let invPath = Path.GetInvalidPathChars ()
    let regexSearch = string(invFile) + string(invPath)
    let r = new Regex(String.Format("[{0}]", Regex.Escape(regexSearch)))

    let root = Path.GetPathRoot(path)
    let path = path.Remove(0, root.Length)

    let path = path.Replace('/', '\\')
    let parts = 
        path.Split(Path.DirectorySeparatorChar)
        |> Array.map (fun part ->
            r.Replace(part, replaceWith)
        )

    root + String.Join(Path.DirectorySeparatorChar.ToString(), parts)