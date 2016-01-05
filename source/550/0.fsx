open System

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
        | [] -> failwith "Error parsing input string."
        | (result,_)::_ -> result
