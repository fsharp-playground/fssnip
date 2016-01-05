[<AutoOpen>]
module Either

type Either<'a, 'b> =
    | Left of 'a
    | Right of 'b

type either<'a, 'b> =
    Either<'a, 'b> // lower-case alias like option