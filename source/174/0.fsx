    let inline (!>) f = () |> fun () -> f
    let x a b = a + b
    let x_arg = !> x