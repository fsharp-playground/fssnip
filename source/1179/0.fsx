/// Represents an open world, in a functional sense.
type IWorld =
    /// Try to find an element of a world.
    abstract member TryFind : Address -> 'e option
    /// Set an element of a world.
    abstract member Set : Address -> 'e -> #IWorld
    /// Remove an element of a world.
    abstract member Remove : Address -> #IWorld

/// The world, in a functional programming sense.
/// A reference type with some value semantics.
and World =
    { Game : Game
      Subscriptions : Subscriptions
      Mailboxes : Mailboxes }
    interface IWorld with
        member this.TryFind address = None
        member this.Set address element = this
        member this.Remove address = this