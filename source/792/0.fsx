let foo() =
    let mutable x = 0
    [for i in [0..10] do
        x <- i
        yield i ]