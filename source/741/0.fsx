open System

// [snippet:Html formatting functions]
open System.Text.RegularExpressions
/// remove html tags to make raw text
let removeTags text = 
  Regex.Replace(text,"<.*?>","")
/// replace some words for correct pronouciation
let replaceWords text =
  [
    @"\(\W\)\." , "\1 "
    @"\.\(\W\)" , " \1"
    "F#"        , "F sharp"
    ";|:"       , " "
  ]
  |> Seq.fold (fun input (pattern,replacement) -> 
    Regex.Replace(input,pattern,replacement)) text


open System.Web
/// unescape some escaped charactors
let unescapeHtml text = HttpUtility.HtmlDecode text


/// do all formatting operations
let formatHtml text =
  text
  |> removeTags
  |> unescapeHtml
  |> replaceWords
//[/snippet]


//[snippet:Get text to speech]
//TODO: please rewrite the below reference to your HtmlAgilityPack.dll
#r @"C:\Users\nagat01\Documents\Visual Studio 11\Projects\WebTrawler\packages\HtmlAgilityPack.1.4.3\lib\HtmlAgilityPack.dll"
open HtmlAgilityPack
/// this xpath specifies the text to speech
let xpath = 
  """
    //div[@id="question-header"]//a
  | //div[@class="post-text"]//p
  | //span[@class="comment-copy"]
  | //a[@class="comment-user"]
  """
let getTextToSheech url =
  let documentNode = HtmlWeb().Load(url).DocumentNode
  seq {
    for text in documentNode.SelectNodes xpath ->
      text.InnerHtml
      |> formatHtml
  }
  |> String.concat "\n"
//[/snippet]


//[snippet:Usage]
#r "System.Speech"  
open System.Speech.Synthesis
let speech url =
  let text = getTextToSheech url
  printfn "%s" text
  use speechSynthesizer = new SpeechSynthesizer(Rate= -3)
  speechSynthesizer.Speak text

let url = "http://stackoverflow.com/questions/181613/hidden-features-of-f"
speech url
//[/snippet]