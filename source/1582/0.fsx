let inline flip f a b = f b a
let inline orElse v =
    function
    | Some x -> Some x
    | None -> v
    
let mplus x = flip orElse x

// Some 1
let a = mplus (Some 1) (Some 3)
// Some 1
let b = mplus (Some 1) (None)
// Some 2
let c = mplus (None) (Some 2)
// None
let d : int option = mplus (None) (None)
