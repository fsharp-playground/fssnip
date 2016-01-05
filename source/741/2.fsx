open System

// [snippet:Html formatting functions]
open System.Text.RegularExpressions
let removeTagsAndReplaceWords text =
  [
    "<a.*?>.*?http.*?</a>" , "url" // replace verbose url to "url"
    "<.*?>"    , ""                // remove html tags
    "F#"       , "F sharp"
  ]
  |> Seq.fold (fun input (pattern,replacement) -> 
    Regex.Replace(input,pattern,replacement)) text


open System.Web
/// unescape some escaped characters
let unescapeHtml text = HttpUtility.HtmlDecode text


/// do all formatting operations
let formatHtml text =
  text
  |> removeTagsAndReplaceWords
  |> unescapeHtml
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
  | //div[@class="user-details"]//a
  | //span[@class="comment-copy"]
  | //a[@class="comment-user"]
  """
let getTextToSheech url =
  let htmlDocument = HtmlWeb().Load(url)
  htmlDocument.DocumentNode.SelectNodes xpath
  |> Seq.map(fun node -> node.InnerHtml |> formatHtml)
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