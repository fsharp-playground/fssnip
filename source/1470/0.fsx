/// Duck typed dereferencing operator. Duck typing enables us to use it with
/// other types besides reference cells, for example, lazy values and options,
/// as well as any of your own types that implement the Value property.
let inline ( ! ) x = (^X : (member get_Value : unit -> _) (x))
