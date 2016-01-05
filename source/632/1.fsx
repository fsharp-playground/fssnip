open System.Data.SqlClient

type SqlHelper (connection) =
    let exec bind parametres query = 
        use conn = new SqlConnection (connection)
        conn.Open()
        use cmd = new SqlCommand (query, conn)
        parametres |> List.iteri (fun i p -> 
                        cmd.Parameters.AddWithValue(sprintf "@p%d"  i, box p) |> ignore)
        bind cmd

    member __.Execute = exec <| fun c -> c.ExecuteNonQuery() |> ignore
    member __.Scalar  = exec <| fun c -> c.ExecuteScalar()
    member __.Read f  = exec <| fun c -> [ let read = c.ExecuteReader()
                                           while read.Read() do 
                                               yield f read ]

// usage
type Customer = { Id : int; Name : string }

let sql = new SqlHelper ("some connection")

let getCustomers cityId minAges = 
    sql.Read (fun r -> { Id = unbox r.[0]; Name = unbox r.[1] })
             [cityId; minAges] 
             "select Id, Name 
              from dbo.Customers 
              where CityId = @p0 and Age > @p1"