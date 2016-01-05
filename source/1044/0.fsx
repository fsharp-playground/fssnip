let Y (Fs : (('a -> 'b) list -> 'a -> 'b) list) : ('a -> 'b) list =
    let refs = [ for _ in Fs -> ref Unchecked.defaultof<'a -> 'b> ]
    let etaExpanded = refs |> List.map (fun fp -> (fun x -> fp.Value x))
    
    Fs 
    |> List.map (fun F -> F etaExpanded)
    |> List.zip refs
    |> List.iter (fun (fp, f) -> fp := f)

    etaExpanded

// example
#nowarn "25"

let [even; odd] =
    let even = fun [even; odd] n -> n = 0 || odd (n-1)
    let odd =  fun [even; odd] n -> n <> 0 && even (n-1)
    Y [even; odd]


odd 2013