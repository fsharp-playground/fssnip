// Generator of change variants for any sum and set of coin denominations
let rec change sum coins =
    if sum = 0 then [[]]
    else 
        match coins with
        | h::t -> 
            let xs = change sum t
            if sum >= h then
                let ys = change (sum - h) coins |> List.map (fun xs -> h :: xs)
                List.append xs ys
            else
                xs
        | [] -> []

// Application to Problem 31
change 200 [200;100;50;20;10;5;2;1]
|> List.length
|> printfn "Project Euler Problem 31 Answer: %d"