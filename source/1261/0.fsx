module Option =
    let either f x = function
        | None -> x
        | Some v -> f v