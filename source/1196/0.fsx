[<AutoOpen>]
module Either
type Either<'a, 'b> = Choice<'a, 'b>

module Either =
    let left = Either.Choice1Of2
    let right = Either.Choice2Of2