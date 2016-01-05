open System
open System.Text.RegularExpressions

let stripHtml (html:string) = 
  Regex.Replace(html, "<.*?>", "")
  
let formatText length (comment:string) =
  let comment = comment.Replace("\n", " ").Replace("\r", " ")
  let short = comment.Substring(0, min length (comment.Length))
  if short.Length < comment.Length then short + "..." else short

let formatDate (date:DateTime) = 
  let ts = DateTime.Now - date
  if ts.TotalDays > 1.0 then sprintf "%d days ago" (int ts.TotalDays)
  elif ts.TotalHours > 1.0 then sprintf "%d hours ago" (int ts.TotalHours)
  elif ts.TotalMinutes > 1.0 then sprintf "%d minutes ago" (int ts.TotalMinutes)
  else "just now"
