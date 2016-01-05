type loop<'a,'b> =
    | Return of 'a
    | Loop of 'b
 
let rec loop vars f =
    match f vars with
    | Return result -> result
    | Loop vars -> loop vars f
 
//Examples:
 
//input lists
let xl, yl = List.init (2000000) id, List.init (2000000) (~-)
 
//merge xl and yl using loop abstraction
//fastest run: Real: 00:00:00.921, CPU: 00:00:00.921, GC gen0: 21, gen1: 12, gen2: 0
let merge = loop (xl, yl, []) (fun (xl,yl,acc) ->
    match xl, yl with
    | [], _ | _, [] -> Return acc //ommitting reversal of acc to keep time stats pure
    | x::xl, y::yl -> Loop(xl,yl,(x,y)::acc))
 
//merge xl and yl using traditional pattern.
//fastest run: Real: 00:00:00.434, CPU: 00:00:00.437, GC gen0: 11, gen1: 7, gen2: 0
let merge_traditional =
    let rec loop xl yl acc =
        match xl, yl with
        | [], _ | _, [] -> acc //ommitting reversal of acc to keep time stats pure
        | x::xl, y::yl -> loop xl yl ((x,y)::acc)
    loop xl yl []

//merge using a possible built-in language feature:
//let merge = loop xl=xl yl=yl acc=[] do
//    match xl, yl with
//    | [], _ | _, [] -> acc
//    | x::xl, y::yl -> loop xl yl ((x,y)::acc)