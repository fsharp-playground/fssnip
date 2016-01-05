type SQLite() =
    let dbexist = File.Exists(NSettings.dbpath)
    let connSetting = sprintf "Data Source=%s;Version=3;New=%s;Compress=True;" 
                        <| NSettings.dbpath 
                        <| if (dbexist) then "False" else "True"
    let openConn() =
        let conn = new SQLiteConnection(connSetting)
        conn.Open()
        conn :> IDbConnection
    let connMgr = Sql.withNewConnection openConn
    let exec a = Sql.execNonQuery(connMgr) a [] |> ignore
    let P = Sql.Parameter.make
    let getNode (dr: IDataRecord) =
        let date, value =   (dr?date).Value 
                            (dr?value).Value
        new dbNode(date, value)
    do
        if not dbexist then
            exec "CREATE TABLE \"archive\" ( \"date\" INTEGER, \"value\" INTEGER)"
            exec "CREATE TABLE \"instant\" ( \"date\" INTEGER, \"value\" INTEGER)"
    member X.execute q = exec q
    member X.query q =
        Sql.execReader(connMgr) q []
        |> Seq.ofDataReader
        |> Seq.map getNode
        |> Seq.toArray