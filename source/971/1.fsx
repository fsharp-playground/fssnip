let n_choose_k n k = let rec choose lo  =
                         function
                         |0 -> [[]]
                         |i -> [for j in lo .. (Array.length n)-1 do
                                     for ks in choose (j+1) (i-1) do
                                       yield n.[j] :: ks ]
                     in choose 0  k                           
                         

n_choose_k [|'a' .. 'f'|] 3 ;;