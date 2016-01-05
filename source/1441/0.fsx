module Html
let selfClosedTag name = sprintf "<%s/>" name
let contentTag name content = sprintf "<%s>%s</%s>" name content name
let emptyTag name= contentTag name System.String.Empty
let heading n content = sprintf "h%d" n |> contentTag <| content
heading 1 "Creating DSLs with F#"
