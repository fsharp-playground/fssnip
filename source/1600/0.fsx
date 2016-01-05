// http://stackoverflow.com/a/12938883/17049
open FParsec

let toLower = System.Char.ToLower
let toUpper = System.Char.ToUpper

let caseInsensitiveChar c = pchar (toLower c) <|> pchar (toUpper c)  
//let caseInsensitiveString (*s*) = ?? (Seq.map caseInsensitiveChar)  