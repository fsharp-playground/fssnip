open System

let decorateSequence concatnator xs mapper decorator =
    xs
    |> List.map mapper
    |> List.fold concatnator ""
    |> decorator

let decorateAttributes xs mapper decorator =
    decorateSequence (fun x y -> x + " " + y) xs mapper decorator

let decorateNodes xs mapper decorator =
    decorateSequence (fun x y -> x + y) xs mapper decorator

let decorateNode (tag : string) s =
    tag + s + (tag.Replace ("<", "</"))

type MetaNode =
    HttpEquiv of string | Content of string

let markupMetaNode = function
    | HttpEquiv s -> "Http-Equiv=\"" + s + "\""
    | Content s   -> "Content=\"" + s + "\""

type HeaderNode =
    Meta of (MetaNode list) | Title of string

let markupHeaderNode = function
    | Meta mls -> decorateAttributes mls markupMetaNode <| fun s -> "<meta" + s + " />"
    | Title s  -> decorateNode "<title>" s

type Header =
    Head of (HeaderNode list)

let markupHeader = function
    | Head hls -> decorateNodes hls markupHeaderNode <| decorateNode "<head>"

type BodyNode =
    | Div of (BodyNode list)
    | Span of (BodyNode list)
    | P of (BodyNode list)
    | Text of string
    | H1 of string | H2 of string | H3 of string
    | Br | Hr

let rec markupBodyNode = function
    | Br -> "<br />"
    | Hr -> "<hr>"
    | H1 s -> decorateNode "<h1>" s
    | H2 s -> decorateNode "<h2>" s
    | H3 s -> decorateNode "<h3>" s
    | Text s -> s
    | Div bls  -> decorateNodes bls markupBodyNode <| decorateNode "<div>"
    | Span bls -> decorateNodes bls markupBodyNode <| decorateNode "<span>"
    | P bls    -> decorateNodes bls markupBodyNode <| decorateNode "<p>"

type Body =
    Body of (BodyNode list)

let markupBody = function
    | Body bls -> decorateNodes bls markupBodyNode <| decorateNode "<body>"

type HtmlNode =
    Html of Header * Body

let markupHtmlNode = function
    | Html (h, b) -> (markupHeader h) + (markupBody b)
                    |> decorateNode "<html>"

type DocType = DocType of string

let markupDocType = function
    | DocType s -> "<!DOCTYPE " + s + ">"

type HtmlFile =
    HtmlFile of (DocType * HtmlNode)

let markupHtmlFile = function
    | HtmlFile (d, h) -> (markupDocType d) + (markupHtmlNode h)

HtmlFile (
    DocType ("html"),
    Html (
        Head (
            [Meta ([HttpEquiv ("Content-Type"); Content ("text/html; charset='utf-8'")]);
            Title ("Html File")]
        ),
        Body (
            [H1 ("This"); H2 ("is"); H3 ("it");
            Hr;
            Div (
                [Span ([Text ("a span node")]); P ([Text ("a paragraph")]); Br;
                Text ("plain text")]
            )]
        )
    )
) |> markupHtmlFile |> printfn "%s"

// You will see like below:
// <!DOCTYPE html><html><head><meta Http-Equiv="Content-Type" Content="text/html; c
// harset='utf-8'" /><title>HtmlBuilder</title></head><body><h1>This</h1><h2>is</h2
// ><h3>it</h3><hr><div><span>a span node</span><p>a paragraph</p><br />plain text<
// /div></body></html>