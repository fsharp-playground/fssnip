let nums = [|1;2;3|]

let initial = [|1;2|]

let pattern = Array.concat [initial ;Array.sub nums (Array.length initial) ((Array.length nums) - (Array.length initial)) ]

match nums with pattern -> "bingo"