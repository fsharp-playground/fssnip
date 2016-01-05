let fromBase26 (num:string) =
    
    let digits = 'Z' :: ['A' .. 'Y']

    let charList = num.ToCharArray() |> Array.toList

    let lookup x =
        List.findIndex (fun z -> z = x) digits

    let rec conv (lst, x:int, y:int) : int =
        match lst with
        | hd :: tl -> conv (tl,(x + (pown digits.Length y) * (lookup hd)), y-1)
        | [] -> x

    conv (charList, 0, num.Length-1)
