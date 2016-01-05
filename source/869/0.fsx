open System
open System.Data.Sql
open System.Data.SqlClient

let SkipIdUsingMappings (dt : Data.DataTable) (bc : SqlBulkCopy) =
    for i in 0..dt.Columns.Count-1 do 
        bc.ColumnMappings.Add(i, i+1) |> ignore