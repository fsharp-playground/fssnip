/// mutable class with auto-properties
type Person(name : string, age : int) =
    /// Full name
    member val Name = name with get, set
    /// Age in years
    member val Age = age with get, set