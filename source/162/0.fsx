let getActiveSQLServers() =
        [for serv in Sql.SqlDataSourceEnumerator.Instance.GetDataSources().Rows ->
            if System.DBNull.Value.Equals(serv.["InstanceName"]) then
                serv.["ServerName"] :?> string
            else
                ( serv.["ServerName"] :?> string + "\\" 
                    + (serv.["InstanceName"] :?> string) ) ].ToList()