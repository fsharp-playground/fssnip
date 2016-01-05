// Factory Pattern
[<AbstractClassAttribute>]
type Connection() =
    abstract member Description :unit -> unit

type OracleConnection() =
    inherit Connection()
    override O.Description() = printfn "I'm Oracle Connection"

type SQLServerConnection() =
    inherit Connection()
    override O.Description() = printfn "I'm SQLServer Connection"

type MySQLConnection() =
    inherit Connection()
    override O.Description() = printfn "I'm MySQL Connection"

type Factory() =
    member O.CreateConnection( connName ) =
        match connName with
        | "Oracle" -> OracleConnection() :> Connection
        | "SQLServer" -> SQLServerConnection() :> Connection
        | "MySQL" -> MySQLConnection() :> Connection
        | _ -> failwith "No such connection "

// Testing
let factory = Factory()
let connection = "Oracle" |> factory.CreateConnection
connection.Description()