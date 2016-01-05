[<AutoOpen>]
module WorldLens
open FSharpx

/// Lens for a particular address in a world.
let forAddress address = 
    { Get = World.tryFind address
      Set = 
          function
          | Some value -> World.set address value
          | None -> World.remove address }