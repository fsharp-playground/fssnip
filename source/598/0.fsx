/// Translate any directional single quotes to ordinary ones
let fixsingleq (s : string) =
    s.Replace('’', '\'').Replace('‘', '\'')

/// Translate any directional double quotes to ordinary ones
let fixdblq (s : string) =
    s.Replace('“', '\"').Replace('”', '\"')

/// Translate any directional quotes to ordinary ones
let fixspecialq =
    fixsingleq >> fixdblq

/// Double any double-quotes
let dblq (s : string) =
    s.Replace("\"", "\"\"")

/// Enclose in double-quotes
let quote (s : string) = sprintf "\"%s\"" (s.Trim())

/// Tabs to spaces
let tabtospace (s : string) = 
    s.Replace('\t', ' ')

/// Remove multiple spaces
let rec singlespace (s : string) = 
    if s.Contains("  ") then
        singlespace (s.Replace("  ", " "))
    else
        s

/// Clean and quote
let cq = tabtospace >> singlespace >> fixspecialq >> dblq >> quote 
