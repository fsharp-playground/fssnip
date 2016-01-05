/// The world, in a functional programming sense.
/// A reference type with some value semantics.
and World =
    { Game : Game
      Subscriptions : Subscriptions
      Mailboxes : Mailboxes }
    interface IWorld with
        member this.TryFind address = None
        member this.Set<'e, 'w when 'w :> IWorld> (address : Address) (element : 'e) : 'w = this
        member this.Remove<'w when 'w :> IWorld> address = this :> IWorld