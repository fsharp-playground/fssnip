/// An active pattern that compares two objects of the same type.
let (|Less|Equal|Greater|) (x, y) =
    let cmp = compare x y
    if cmp < 0 then Less
    elif cmp > 0 then Greater
    else Equal
