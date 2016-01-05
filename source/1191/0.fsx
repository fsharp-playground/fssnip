// based on 
// https://github.com/mackwic/To.ml
// https://github.com/seliopou/toml

open System
open System.Globalization

[<RequireQualifiedAccess>]
type NodeArray =
  | Bools   of bool   list
  | Ints    of int    list
  | Floats  of float  list
  | Strings of string list
  | Dates   of DateTime list
  | Arrays  of NodeArray list 
  override __.ToString () =
    let inline f xs = List.map string xs |> String.concat ", "
    match __ with
    | Bools   bs -> f bs 
    | Ints    is -> f is 
    | Floats  fs -> f fs
    | Strings ss -> f ss
    | Dates   ds -> f ds
    | Arrays ars -> f ars

[<RequireQualifiedAccess>]
type TomlValue =
  | Bool   of bool
  | Int    of int
  | Float  of float
  | String of string
  | Date   of DateTime
  | Array  of NodeArray
  override __.ToString () =
    match __ with
    | Bool   b -> sprintf "TBool(%b)"   b
    | Int    i -> sprintf "TInt(%d)"    i
    | Float  f -> sprintf "TFloat(%f)"  f
    | String s -> sprintf "TString(%s)" s
    | Date   d -> sprintf "TDate(%A)" d
    | Array  a -> sprintf "[%O]" a

type Token = KeyGroup of string list | KeyValue of string * TomlValue

open FParsec

let spc = many (anyOf [' '; '\t']) |>> ignore
let lexeme p = p .>> spc
let comment = pchar '#' .>>. restOfLine false |>> ignore
let line p = p .>> lexeme newline
let blanks = lexeme (skipMany ((comment <|> spc) .>> lexeme newline))


let ls s = lexeme <| pstring s
let zee    = ls "Z"
let quote  = ls "\""
let lbrace = pstring "[" .>> spaces
let rbrace = pstring "]" .>> spaces
let comma  = pstring "," .>> spaces
let period = ls "."
let equal  = ls "="
let ptrue  = ls "true"  >>% true
let pfalse = ls "false" >>% false

let pdate' = 
    fun s -> 
      try preturn (DateTime.Parse (s, null, DateTimeStyles.RoundtripKind))               
      with _ -> fail "date format error"


let pbool  = ptrue <|> pfalse <?> "pbool"
let pstr   = between quote quote (manySatisfy ((<>)'"')) <?> "pstr"
let pint   = attempt pint32 <?> "pint"
let pfloat = attempt pfloat <?> "pfloat"
let pdate  = attempt (spc >>. anyString 20 .>> spc >>= pdate') <?> "pdate"

let parray elem = attempt (between lbrace rbrace (sepBy (elem .>> spaces) comma))
let pboolarray  = parray pbool  |>> NodeArray.Bools   <?> "pboolarray"
let pdatearray  = parray pdate  |>> NodeArray.Dates   <?> "pdatearray"
let pintarray   = parray pint   |>> NodeArray.Ints    <?> "pintarray"
let pstrarray   = parray pstr   |>> NodeArray.Strings <?> "pstrarray"
let pfloatarray = parray pfloat |>> NodeArray.Floats  <?> "pfloatarray"
let rec parrayarray = 
  parray (pboolarray <|> pdatearray <|> pintarray <|> pstrarray <|> pfloatarray) 
  |>> NodeArray.Arrays <?> "parrayarray"

let value = 
  (pbool       |>> TomlValue.Bool ) <|> 
  (pdate       |>> TomlValue.Date ) <|> 
  (pstr        |>> TomlValue.String)<|> 
  (pfloat      |>> TomlValue.Float) <|> 
  (pint        |>> TomlValue.Int  ) <|> 
  (pboolarray  |>> TomlValue.Array) <|>
  (pdatearray  |>> TomlValue.Array) <|>
  (pintarray   |>> TomlValue.Array) <|>
  (pstrarray   |>> TomlValue.Array) <|>
  (pfloatarray |>> TomlValue.Array) <|>
  (parrayarray |>> TomlValue.Array)
  
let keyvalue = 
  let key = many1Chars (noneOf " \t\n=")
  lexeme key .>>. (equal >>. value) |>> KeyValue

let keygroup = 
  let key = lexeme (many1Chars (noneOf " \t\n]."))
  blanks >>. between lbrace rbrace (sepBy key period) |>> KeyGroup

let document = blanks >>. many (keygroup <|> keyvalue .>> blanks)


let example = (*[omit:(...)]*) """
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

open System.Collections.Generic
let toml = Dictionary<string, TomlValue>()
let mutable currentKeyGroup = None

try
let result = run document example
match result with
| Success(tokens,_,_) ->
  for token in tokens do
    match token with
    | KeyGroup kg -> currentKeyGroup <- Some kg
    | KeyValue (key,value) -> 
      let key = 
        seq {
        if currentKeyGroup.IsSome then
          yield! currentKeyGroup.Value
        yield key
        } |> String.concat "."
      toml.Add(key, value)
| _ -> ()

with e -> ()

for token in toml do
  printfn "%A" token