      //Given any of this kind (record)
      type product = 
        {ProductId:int;
         ProductName:string;
         SupplierID:int;
         CategoryID:int;
         QuantityPerUnit:string;
         UnitPrice:decimal;
         UnitsInStock:int;
         UnitsOnOrder:int;
         ReorderLevel:int;
         Discontinued:bool}
         
      
      //Read all records from a table an give an IEnumerable collection as output
      let getAllRecords<'a>(tableName) (conn) =
            use cmd = new SqlCommand(("Select * from " + tableName), conn)
            use reader = cmd.ExecuteReader()
            let recFields = typeof<'a>.GetMembers() |> Array.filter (fun (f:MemberInfo) -> f.MemberType.ToString() = "Property")
            [while reader.Read() do yield (recFields |> Array.map (fun (f:MemberInfo) -> unbox (reader.[f.Name])))] 
            |> List.map (fun oArray -> Activator.CreateInstance(typeof<'a>, oArray))
            |> Seq.ofList |> Seq.map (fun o -> o :?> 'a)        