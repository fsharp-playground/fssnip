    let toUnsigned x = 
        if x >= 0 then 
            2*x
        else
            (-2*x) - 1
    let toSigned x =
        if x % 2 = 0 then
            x/2
        else
            (x+1)/(-2)