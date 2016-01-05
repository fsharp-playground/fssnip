module World

/// Specifies the address of an element in a world.
/// Note that, similar to Mu, the last node of the address is the name of the event (such as "clicked").
/// Also note that subscribing to a partial address results in listening to all messages whose
/// beginning address nodes match the partial address (sort of a wild-card).
/// A value type.
type Address = Lun list

/// Represents an open world, in a functional sense.
type IWorld =
    /// Try to find an element of a world.
    abstract member TryFind<'e> : Address -> 'e option
    /// Set an element of a world.
    abstract member Set<'e, 'w when 'w :> IWorld> : Address -> 'e -> 'w
    /// Remove an element of a world.
    abstract member Remove<'w when 'w :> IWorld> : Address -> 'w

/// Try to find an element of a world.
let inline tryFind<'w when 'w :> IWorld> address (world : 'w) =
    world.TryFind address

(* I might rather drop the interface and use generics like this -
let inline tryFind<'w when 'w : (member TryFind<'e> : Address -> 'e)> address (world : 'w) =
    world.TryFind<'e> address*)

/// Set an element of a world.
let inline set<'w when 'w :> IWorld> address element (world : 'w) =
    world.Set address element
    
/// Remove an element of a world.
let inline remove<'w when 'w :> IWorld> address (world : 'w) =
    world.Remove address