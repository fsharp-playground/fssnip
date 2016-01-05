#r "WindowsBase"
#r "PresentationCore"
open System
open System.Text.RegularExpressions
open System.Windows
let indentConverter before after =
  let spaces n = String(' ',n)
  Regex.Replace(Clipboard.GetText(), spaces before, spaces after)
  |> Clipboard.SetText
indentConverter 4 2