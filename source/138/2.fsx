// [snippet: Monadic parser combinators for parsing a Uri.]
// NOTE: Requires Cashel: https://github.com/panesofglass/cashel
open System
open Cashel.Parser
open Cashel.ArraySegmentPrimitives

module Primitives =
  open Hex // Daniel's snippet at http://fssnip.net/25

  let ch = matchToken
  let str = matchTokens

  let token = any [ 0uy..127uy ]
  let upper = any [ 'A'B..'Z'B ]
  let lower = any [ 'a'B..'z'B ]
  let alpha = upper +++ lower
  let digit = any [ '0'B..'9'B ]
  let digitval = parse {
    let! d = digit
    return int d - int 48uy }
  let alphanum = alpha +++ digit
  let control = any [ 0uy..31uy ] +++ ch 127uy
  let tab = ch '\t'B
  let lf = ch '\n'B
  let cr = ch '\r'B
  let crlf = str (List.ofSeq "\r\n"B)
  let space = ch ' 'B
  let dquote = ch '"'B
  let hash = ch '#'B
  let percent = ch '%'B
  let plus = ch '+'B
  let hyphen = ch '-'B
  let dot = ch '.'B
  let colon = ch ':'B
  let slash = ch '/'B
  let qmark = ch '?'B
  let xupper = any [ 'A'B..'F'B ]
  let xlower = any [ 'a'B..'f'B ]
  let xchar = xupper +++ xlower
  let xdigit = digit +++ xchar
  let escaped = parse {
    do! forget percent
    let! d1 = xdigit
    let! d2 = xdigit
    let hex = fromHexDigit (char d1) <<< 4 ||| fromHexDigit (char d2)
    return byte hex }

module UriParser =
  open Primitives

  let scheme = parse {
    let! init = alpha
    let! more = alpha +++ digit +++ plus +++ hyphen +++ dot |> repeat1
    return init::more }
  let mark = any [ '-'B;'_'B;'.'B;'!'B;'~'B;'*'B;'''B;'('B;')'B ]
  let reserved = any [ ';'B;'/'B;'?'B;':'B;'@'B;'&'B;'='B;'+'B;'$'B;','B ]
  let unreserved = alpha +++ mark
  let pchar = unreserved +++ escaped +++ any [ ':'B;'@'B;'&'B;'='B;'+'B;'$'B;'.'B ]
  let uric = reserved +++ unreserved +++ escaped
  let uricNoSlash = unreserved +++ escaped +++ any [ ';'B;'?'B;':'B;'@'B;'&'B;'='B;'+'B;'$'B;','B ]
  let relSegment = unreserved +++ escaped +++ any [ ';'B;'@'B;'&'B;'='B;'+'B;'$'B;','B ] |> repeat1
  let regName = unreserved +++ escaped +++ any [ '$'B;','B;';'B;':'B;'@'B;'&'B;'='B;'+'B ] |> repeat1
  let userInfo = unreserved +++ escaped +++ any [ ';'B;':'B;'&'B;'='B;'+'B;'$'B;','B ] |> repeat

  /// param returns a series of pchar.
  let param = pchar |> repeat
  /// segment returns a list of ;-separated params.
  let segment = parse {
    let! hd = param
    let! tl = ch ';'B >>= (fun c1 -> param >>= (fun c2 -> result (c1::c2))) |> repeat
    return (hd::tl |> List.concat) }
  /// pathSegments returns a list of /-separated segments.
  let pathSegments = parse {
    let! hd = segment
    let! tl = ch '/'B >>= (fun s1 -> segment >>= (fun s2 -> result (s1::s2))) |> repeat
    return (hd::tl |> List.concat) }

  let uriAbsPath = parse {
    let! p1 = ch '/'B
    let! p2 = pathSegments
    return p1::p2 }
  let relPath = parse {
    let! hd = relSegment
    let! tl = !? uriAbsPath
    match tl with | Some(t) -> return hd @ t | _ -> return hd }

  let uriQuery = uric |> repeat
  let uriFragment = uric |> repeat

  let ipv4Address = parse {
    let d = '.'B
    let ``digit 1-3`` = digit |> repeat1While (fun ds -> ds.Length < 3)
    let! d1 = ``digit 1-3`` .>> dot
    let! d2 = ``digit 1-3`` .>> dot
    let! d3 = ``digit 1-3`` .>> dot
    let! d4 = ``digit 1-3`` 
    return d1 @ d::d2 @ d::d3 @ d::d4 }

  let private _label first = parse {
    let! a1 = first
    let! a2 = alphanum +++ hyphen |> repeat
    return a1::a2 }
// TODO: Ensure the last item is not a hyphen.
//    match a1::a2 with
//    | hd::[] as res -> return res
//    | res ->
//        let! a3 = alphanum
//        return res @ [a3] }
  let topLabel = _label alpha
  let domainLabel = _label alphanum
  let hostname = parse {
    let! dl = !?(domainLabel >>= (fun d -> dot >>= (fun dt -> result (d @ [dt]))))
    let! tl = topLabel .>> !?dot
    match dl with Some(sub) -> return sub @ tl | _ -> return tl }

  let host = hostname +++ ipv4Address
  let port = digit |> repeat
  
  let hostport = parse {
    let! h = host
    let! p = !?(colon >>= (fun c -> port >>= (fun p -> result (c::p))))
    match p with Some(v) -> return h @ v | _ -> return h }
  let server = parse {
    let! ui = !?(userInfo >>= (fun ui -> ch '@'B >>= (fun a -> result (ui @ [a]))))
    let! hp = hostport
    match ui with Some(info) -> return info @ hp | _ -> return hp }
  let uriAuthority = server +++ regName
  let netPath = parse {
    let! slash = str (List.ofSeq "//"B)
    let! authority = uriAuthority
    let! absPath = !?uriAbsPath
    let domain = slash @ authority
    match absPath with Some(path) -> return domain @ path | _ -> return domain }

  let opaquePart = parse {
    let! u1 = uricNoSlash
    let! u2 = uric |> repeat
    return u1::u2 }
  let hierPart = parse {
    let! path = netPath +++ uriAbsPath
    let! query = !?(qmark >>= (fun m -> uriQuery >>= (fun q -> result (m::q))))
    match query with Some(qry) -> return path @ qry | _ -> return path }

  let absoluteUri = parse {
    let! s = scheme
    let! c = colon
    let! rest = hierPart +++ opaquePart
    return s @ c::rest }
  let relativeUri = parse {
    let! path = uriAbsPath +++ relPath
    let! query = !?(qmark >>= (fun m -> uriQuery >>= (fun q -> result (m::q))))
    match query with Some(qry) -> return path @ qry | _ -> return path }

  let uriReference = parse {
    let! uri = !?(absoluteUri +++ relativeUri)
    let! frag = !?(hash >>= (fun h -> uriFragment >>= (fun f -> result (h::f))))
    match uri with
    | None ->
        match frag with
        | None -> return []
        | Some(f) -> return f
    | Some(u) ->
        match frag with
        | None -> return u
        | Some(f) -> return u @ f }
// [/snippet]

// [snippet: Usage]
let v = uriReference (ArraySegment<_>("http://ryan:riley@wizardsofsmart.net:8090?query=this#mark"B));;
new System.String(v |> Option.get |> fst |> List.map char |> List.toArray);;

let v = uriReference (ArraySegment<_>("#mark"B));;
new System.String(v |> Option.get |> fst |> List.map char |> List.toArray);;
// [/snippet]