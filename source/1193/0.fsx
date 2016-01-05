module Toml
// based on https://github.com/mackwic/To.ml and https://github.com/seliopou/toml
open System
open FParsec
type Token = KeyGroup of string list | KeyValue of string * obj

let (<||>) p1 p2 = attempt (p1 |>> box) <|> attempt (p2 |>> box)
let spc      = many (anyOf [' '; '\t']) 
let lexeme s = pstring s .>> spc
let lexemel s= pstring s .>> spaces
let comment  = pchar '#' .>>. restOfLine false 
let blanks   = skipMany ((comment <||> spc) .>> newline .>> spc) .>> spc
let brace p  = between (lexemel "[") (lexemel "]") p
let pbool    = (lexeme "true" >>% true) <|> (lexeme "false" >>% false)
let pstr     = between (lexeme "\"") (lexeme "\"") (manySatisfy ((<>)'"'))
let pdate' s = try preturn (DateTime.Parse (s, null, Globalization.DateTimeStyles.RoundtripKind)) with _ -> fail ""
let pdate    = between spc spc (anyString 20) >>= pdate'
let ary elem = brace (sepBy (elem .>> spaces) (lexemel ","))
let pary     = ary pbool <||> ary pdate <||> ary pint32 <||> ary pstr <||> ary pfloat
let value    = pbool <||> pdate <||> pstr <||> pfloat <||> pint32 <||> pary <||> ary pary
let kvKey    = many1Chars (noneOf " \t\n=")
let keyvalue = (kvKey .>> spc) .>>. (lexeme "=" >>. value) |>> KeyValue
let kgKey    = (many1Chars (noneOf " \t\n].")) .>> spc
let keygroup = blanks >>. brace (sepBy kgKey (lexeme ".")) |>> KeyGroup
let document = blanks >>. many (keygroup <|> keyvalue .>> blanks)

let parse text =
  let toml = Collections.Generic.Dictionary<string, obj>()
  let currentKg = ref []
  match run document text with
  | Success(tokens,_,_) ->
    for token in tokens do
      match token with
      | KeyGroup kg -> currentKg := kg
      | KeyValue (key,value) -> 
        let key = String.concat "." [ yield! !currentKg; yield key]
        toml.Add(key, value)
  | __ -> ()
  toml

let example = (*[omit:toml example]*) """
[group1]
key = true
key2 = 1337
title = "TOML Example"

[  owner]
name = "Tom Preston-Werner"
organization = "GitHub"
bio = "GitHub Cofounder & CEO\nLikes tater tots and beer."
dob =  1979-05-27T07:32:00Z   # First class dates? Why not?

[database  ]
server= "192.168.1.1"
ports       =  [ 8001,8001 , 8002]
connection_max =5000
enabled=true

[servers]

  # You can indent as you please. Tabs or spaces. TOML don't care.
  [  servers  .alpha]
  ip = "10.0.0.1"
  dc = "eqdc10"

  [servers.  beta  ]
  ip = "10.0.0.2"
  dc = "eqdc10"

[clients]
data = [ ["gamma","delta"], [1, 2] ] # just an update to make sure parsers support it

# Line breaks are OK when inside arrays
hosts = [
  "alpha",
  "omega"
  ]
""" (*[/omit]*)

for tomlValue in parse example do
  printfn "%A" tomlValue
Console.ReadLine() |> ignore