(* Define a OptionalBuilder, which allows the convenient use of computation expressions to work on Option types. *)
type OptionalBuilder() =
    member this.Bind(value, func) = value |> Option.map(func)
    member this.Return(value) = Some(value)
    member this.Zero() = Some ()
    
let optional = OptionalBuilder()

(* Define an active pattern to wrap reference types in Option *)
let (|Optional|) value = if value = null then None else Some(value)

(* Define a type that can be called from C# without needing to wrap in Option first. *)
type Foo() =
    static member Bar(Optional value) =
        optional {
            let! message = value
            printfn "%s" message
        }

(* Example usage *)
Foo.Bar("Hello, world!") //Prints, "Hello, world!"
Foo.Bar(null) //Does nothing