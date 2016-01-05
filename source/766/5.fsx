[<AutoOpen>]
module FsDataEx =

    open System
    open System.Data
    open System.Data.SqlClient

    type SqlCommand with
        member this.AsyncExecuteNonQuery() =
            Async.FromBeginEnd(this.BeginExecuteNonQuery, this.EndExecuteNonQuery)

        member this.AsyncExecuteReader() =
            Async.FromBeginEnd(this.BeginExecuteReader, this.EndExecuteReader)

        member this.AsyncExecuteReader(behavior: CommandBehavior) =
            Async.FromBeginEnd((fun (cb, state) -> this.BeginExecuteReader(cb, state, behavior)), 
                               this.EndExecuteReader)

        member this.AsyncExecuteXmlReader() =
            Async.FromBeginEnd(this.BeginExecuteXmlReader, this.EndExecuteXmlReader)

    /// Turns a null reference into DBNull.
    let private asDbNull (value:obj) =
        if Object.ReferenceEquals(value, null) 
        then box DBNull.Value else value

    open System.Data.Objects
    open System.Data.EntityClient

    type System.Linq.IQueryable<'a> with
        /// Executes an Entity Framework query asynchronously, provided the underlying data 
        /// provider is System.Data.SqlClient.
        member this.AsyncExecute<'a when 'a :> DataClasses.EntityObject>() = async {
            let query = this :?> ObjectQuery<'a>
            let connection = (query.Context.Connection :?> EntityConnection).StoreConnection :?> SqlConnection
            let builder = SqlConnectionStringBuilder(connection.ConnectionString, AsynchronousProcessing = true)
            connection.ConnectionString <- builder.ConnectionString
            
            use cmd = connection.CreateCommand()
            cmd.CommandText <- query.ToTraceString()
            
            query.Parameters
            |> Seq.map (fun x -> SqlParameter(x.Name, asDbNull x.Value))
            |> Array.ofSeq
            |> cmd.Parameters.AddRange

            // The connection should be closed when the EF DataContext is disposed.
            connection.Open()

            let! reader = cmd.AsyncExecuteReader()

            return seq {
                // The ObjectResult returned by Translate can be enumerated
                // only once, and it must be disposed
                use result = query.Context.Translate<'a>(reader)
                yield! result
            } |> Seq.cache
        }