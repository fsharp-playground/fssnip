// Decorator Pattern
[<AbstractClassAttribute>]
type ComputerParts() =
    abstract member Description :unit -> unit

type Computer() =
    inherit ComputerParts()
    override O.Description() = printf "I'm a Computer with"

type CDROM( c :ComputerParts ) =
    inherit ComputerParts()
    override O.Description() = c.Description(); printf ", CDROM"

type Mouse( c :ComputerParts ) =
    inherit ComputerParts()
    override O.Description() = c.Description(); printf ", Mouse"

type Keyboard( c :ComputerParts ) =
    inherit ComputerParts()
    override O.Description() = c.Description(); printf ", Keyboard"

let mutable computer = Computer() :> ComputerParts
computer <- Mouse( computer )
computer <- CDROM( computer )
computer <- Keyboard( computer )

computer.Description()