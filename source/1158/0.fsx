module FParsecCombinators

open FParsec
open System

exception Error of string

type Arg = 
    | WithType of string * string
    | NoType of string

type LocaleContents = 
    | Argument of Arg
    | Text of string
    | Line of LocaleContents list
    | Comment of string
    | NewLine

type Locale =
    | Entry of string * LocaleContents
    | IgnoreEntry of LocaleContents

(*
    Utilities
*)

let brackets = isAnyOf ['{';'}']
    
 (* non new line space *)  
let regSpace = manySatisfy (isAnyOf [' ';'\t'])
  
(* any string literal that is charcaters *)
let phrase = many1Chars (satisfy (isNoneOf ['{';'\n']))
  
let singleWord = many1Chars (satisfy isDigit <|> satisfy isLetter <|> satisfy (isAnyOf ['_';'-']))

(* utility method to set between parsers space agnostic *)
let between x y p = pstring x >>. regSpace >>. p .>> regSpace .>> pstring y

(*
    Arguments
*)

let argDelim = pstring ":"

let argumentNoType = singleWord |>> NoType

let argumentWithType = singleWord .>>.? (argDelim >>. singleWord) |>> WithType

let arg = (argumentWithType <|> argumentNoType) |> between "{" "}" |>> Argument

(*
    Text Elements
*)

let textElement = phrase |>> Text

let newLine = (unicodeNewline >>? regSpace >>? pstring "=") >>% NewLine

let line = many (regSpace >>? (arg <|> textElement <|> newLine)) |>> Line

(*
    Entries
*)

let delim = regSpace >>. pstring "=" .>> regSpace

let identifier = regSpace >>. singleWord .>> delim

let localeElement = unicodeSpaces >>? (identifier .>>. line .>> skipRestOfLine true) |>> Entry

(*
    Comments
*)

let comment = pstring "#" >>. restOfLine false |>> Comment

let commentElement = unicodeSpaces >>? comment |>> IgnoreEntry

(*
    Full Locale
*)

let locale = many (commentElement <|> localeElement) .>> eof

let test input = match run locale input with
                    | Success(r,_,_) -> r
                    | Failure(r,_,_) -> 
                            Console.WriteLine r
                            raise (Error(r))