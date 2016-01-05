// [snippet:get .fs files in .fsproj file]
open System
open System.IO
open System.Text
open System.Text.RegularExpressions

let getFsFilesIn projectDirectory =
   
  let fsprojFile = 
    Directory.GetFiles projectDirectory
    |> Seq.tryFind(fun file -> (FileInfo file).Extension = ".fsproj")

  if fsprojFile.IsSome then
    let content = File.ReadAllText fsprojFile.Value
    let matches = Regex.Matches(content,@"<Compile Include=""(\w+\.fs+)\"" />")
    seq { for m in matches -> m.Groups.[1].Value }
    |> Seq.map(fun filename -> sprintf @"#load ""%s\%s""" projectDirectory filename)
    |> String.concat "\n"
  else failwith ".fsproj file couldn't find !!!"
// [/snippet]

// [snippet:Usage]
// 1. generate #load directive strings
getFsFilesIn @"(Your F# project directory)"
// 2. get them from the Clipboard
|> System.Windows.Forms.Clipboard.SetText

// 3. put them to a script file
// #load "hoge.fs"
// #load "foo.fs"
// #load "bar.fs"
// ..
// [/snippet]