open System

(*[omit:(Parser Monad and combinators omitted. Code available here: http://fssnip.net/8S)]*)
type 'a Parser = Parser of (char list -> ('a * char list) list)
let parse (Parser p) = p 

type ParserBuilder () =
    member x.Return a = Parser (fun cs -> [a, cs])
    member x.Bind (p, f) = Parser (fun cs -> 
        match parse p cs with
        | (c', cs')::_ -> parse (f c') cs'
        | [] -> []
    )
    member x.Zero () = Parser (fun _ -> [])
    member x.ReturnFrom a = a

let parser = ParserBuilder() 

let item = Parser (function [] -> [] | c::cs -> [c, cs])
let sat pred = parser {
    let! c = item
    if pred c then return c
}
let tChar c = sat ((=) c)

/// Concatenates the results of applying parser p and parser q
let (<+>) p q = Parser (fun cs -> (parse p cs) @ (parse q cs))
/// Applies parser p or parser q and returns at most one result
let (<|>) p q = Parser (fun cs -> 
    match (parse (p <+> q) cs) with
    | []    -> []
    | x::xs -> [x]
)

/// Given a char list, returns a parser that parsers it
let rec text = function
    | []  -> parser { return [] }
    | c::cs -> parser { 
        let! _ = tChar c
        let! _ = text cs
        return c::cs 
    } 

/// Combines many (0 or more) applications of parser p
let rec many p = (many1 p) <|> parser { return [] }
/// Combines at least one (1 or more) applications of parser p
and many1 p = 
    parser { 
        let! r = p
        let! rs = many p
        return r::rs
    } 

/// Combines 0 or more applications of parser p separated by parser sep
let rec sepby p sep =  (sepby1 p sep) <|> parser { return [] }
/// Combines 1 or more applications of parser p separated by parser sep
and sepby1 p sep = 
    parser {
        let! r = p
        let! rs = many (parser { 
            let! _ = sep
            return! p
        })
        return r::rs
    } 

/// Chain 0 or more applications of parser p separated by applications of parser op
let rec chainl p op a = (chainl1  p op) <|> parser { return a }
/// Chain 1 or more applications of parser p separated by applications of parser op
and chainl1 p op =  
    let rec rest r = 
        parser {
            let! f = op
            let! r' = p
            return! rest (f r r')
        } <|> parser {return r}

    parser { let! a = p in return! rest a }

let isSpace =
    // list of "space" chars based on 
    // http://hackage.haskell.org/packages/archive/base/latest/doc/html/Data-Char.html#v:isSpace
    let cs = [' '; '\t'; '\n'; '\r'; '\f'; '\v'] |> Set.ofList
    cs.Contains
let space = many (sat isSpace)

let token p = parser { 
    let! r = p
    let! _ = space
    return r
}

let symb = text >> token

let apply p = parse (parser {
    let! _ = space
    let! r = p
    return r
})

let s2cs = List.ofSeq
let cs2s cs = new String(Array.ofList cs)

let runParser p = 
    s2cs >>
    apply p >>
    function
        | [] -> failwith "Error parsing string"
        | (result,_)::_ -> result

(*[/omit]*)

// New parser combinators/helpers
let isHexDigit = 
    let ds = ['A'..'F'] @ ['a'..'f'] @ ['0'..'9'] |> Set.ofList
    ds.Contains
let hexDigit = sat isHexDigit

let charToken = tChar >> token

let betweenChars c1 c2 f = parser {
    let! _ = charToken c1
    let! r = f()
    let! _ = charToken c2
    return r
}

let zeroOrOne p = parser { let! ret = p in return ret } <|> parser { return [] }

let (<@>) p q = parser {
    let! rp = p
    let! rq= q
    return (rp @ rq)
}


//
// JSON Paser Monad
//
#nowarn "40"

type JSValue = 
    | JSString of string
    | JSNumber of float
    | JSObject of (JSValue * JSValue) list
    | JSArray of JSValue list
    | JSBool of bool
    | JSNull

let jsNull =
    let nullLiteral = s2cs "null"
    parser { let! _ = symb nullLiteral in return JSNull}

let jsBool = 
    let trueLit = "true" |> s2cs
    let falseLit = "false" |> s2cs
    parser { let! _ = symb trueLit in return JSBool(true) } 
    <|> parser { let! _ = symb falseLit in return JSBool(false) }

let jsNumber = 
    let digitsParser = many1 (sat System.Char.IsDigit)
    let intParser = (zeroOrOne (text ['-'])) <@> digitsParser
    let fracParser = text ['.'] <@> digitsParser
    let expParser = 
        (text ['e'] <|> text ['E']) <@> 
        (zeroOrOne (text ['+'] <|> text ['-'])) <@>
        digitsParser
    parser {
        let! result = intParser <@> zeroOrOne fracParser <@> zeroOrOne expParser 
        let! _ = space
        return (result |> cs2s |> System.Double.Parse |> JSNumber)
    }     

let jsString = 
    let isChar c = (c <> '\"') && (c <> '\\')
    let isEscChar = 
        let cs = "\"\\/bfnrt" |> List.ofSeq |> Set.ofList
        cs.Contains
    let replaceEscChar = function 'b' -> '\b' | 'f' -> '\f' | 'n' -> '\n'
                                | 'r' -> '\r' | 't' -> '\t' | other -> other
    let escChars = parser {
        let! _ = tChar '\\'
        let! c = sat isEscChar
        return (replaceEscChar c)
    }
    let uniChars = parser {
        let! _ = text [ '\\'; 'u' ]
        let! d1 = hexDigit
        let! d2 = hexDigit
        let! d3 = hexDigit
        let! d4 = hexDigit
        let r = 
            let s = new String [|d1; d2; d3; d4|]
            Byte.Parse(s, Globalization.NumberStyles.HexNumber)
            |> char
        return r
    }
    let chars = many ((sat isChar) <|> escChars <|> uniChars)
    parser {
        let! cs = betweenChars '\"' '\"' (fun () -> chars)
        return (cs |> cs2s |> JSString)
    }

let rec jsValue = jsString <|> jsNumber <|> jsArray <|> jsBool <|> jsNull <|> jsObject
and jsElements = sepby jsValue (charToken ',')
and jsArray = parser {
    let! values = betweenChars '[' ']' (fun () -> jsElements)
    return (JSArray values)
    }
and jsMembers = sepby jsPair (charToken ',')
and jsPair = parser {
    let! key = jsString
    let! _ = charToken ':'
    let! value = jsValue
    return (key, value)
    }
and jsObject = parser {
    let! members = betweenChars '{' '}' (fun () -> jsMembers)
    return (JSObject members)
    }

let parseJson : (string -> JSValue) = runParser jsObject


////////////////////////////////////////////
// Sample JSON from http://json.org/example
let widgetJson = "{\"widget\": {
    \"debug\": \"on\",
    \"window\": {
        \"title\": \"Sample Konfabulator Widget\",        
        \"name\": \"main_window\",        
        \"width\": 500,        
        \"height\": 500
    },    \"image\": { 
        \"src\": \"Images/Sun.png\",
        \"name\": \"sun1\",        
        \"hOffset\": 250,        
        \"vOffset\": 250,        
        \"alignment\": \"center\"
    },    \"text\": {
        \"data\": \"Click Here\",
        \"size\": 36,
        \"style\": \"bold\",        
        \"name\": \"text1\",        
        \"hOffset\": 250,        
        \"vOffset\": 100,        
        \"alignment\": \"center\",
        \"onMouseUp\": \"sun1.opacity = (sun1.opacity / 100) * 90;\"
    }
}}"

// Testing
let jsonAst = parseJson widgetJson
