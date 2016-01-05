module Set

    type Tree<'a when 'a:comparison> =
        | Empty
        | Tree of Tree<'a>*'a*Tree<'a> 

    let rec isMember value tree =
        match (value,tree) with
        | (_,Empty) -> false
        | (x,Tree(a,y,b)) ->
            if value < y then
                isMember x a
            elif value > y then
                isMember x b
            else
                true

    let rec insert value tree = 
        match (value,tree) with
        | (_,Empty) -> Tree(Empty,value,Empty)
        | (v,Tree(a,y,b)) as s -> 
            if v < y then
                Tree(insert v a,y,b)
            elif v > y then
                Tree(a,y,insert v b)
            else
                snd s
