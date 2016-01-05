// [snippet:Micro ORM]
namespace XMicroOrm

open System
open System.Data.Common
open Microsoft.FSharp.Reflection

[<AutoOpen>]
module Extensions =
    type System.Data.Common.DbProviderFactory with
        member f.CreateConnection(connectionString) =
            let cnn = f.CreateConnection()
            cnn.ConnectionString <- connectionString
            cnn

    type System.Data.IDbConnection with
        member f.CreateCommand(connection, commandText) =
            let cmd = f.CreateCommand()
            cmd.Connection  <- connection
            cmd.CommandText <- commandText
            cmd

[<AutoOpen>]
module (* internal *) Helper =
    let toOptionDynamic (typ: Type) (value: obj) =
        let opttyp = typedefof<Option<_>>.MakeGenericType([|typ|])
        let tag, varr = if DBNull.Value.Equals(value) then 0, [||] else 1, [|value|]
        let case = FSharpType.GetUnionCases(opttyp) |> Seq.find (fun uc -> uc.Tag = tag)
        FSharpValue.MakeUnion(case, varr)

    let optionTypeArg (typ : Type) =
        let isOp = typ.IsGenericType && typ.GetGenericTypeDefinition() = typedefof<Option<_>>
        if isOp then Some (typ.GetGenericArguments().[0]) else None

type DbEntityField = { Index: int; Name: string; Type: Type }

module DbEntity =
    let name<'R when 'R : not struct> =
        let rty = typeof<'R>
        assert (FSharpType.IsRecord(rty))
        rty.Name

    let fields<'R when 'R : not struct> =
        let rty = typeof<'R>
        assert (FSharpType.IsRecord(rty))
        FSharpType.GetRecordFields(rty)
        |> Seq.mapi (fun i p -> {Index=i; Name=p.Name; Type=p.PropertyType})

    let read<'R when 'R : not struct> (cnn: DbConnection) sql =
        let rty = typeof<'R>
        assert (FSharpType.IsRecord(rty))
        let makeEntity =
            let mk = FSharpValue.PreComputeRecordConstructor(rty)
            (fun vals -> mk vals :?> 'R)
        let fields = dict <| seq { for fld in fields<'R> -> fld.Name, fld }
        seq { use cmd = cnn.CreateCommand(cnn, sql)
              use reader = cmd.ExecuteReader()
              while reader.Read() do
              yield seq { 0..reader.FieldCount-1 }
                    |> Seq.map (fun i -> reader.GetName(i), reader.GetValue(i))
                    |> Seq.sortBy (fun (n, _) -> fields.[n].Index)
                    |> Seq.map    (fun (n, v) -> match optionTypeArg fields.[n].Type with
                                                 | Some t -> toOptionDynamic t v
                                                 | None   -> v)
                    |> Seq.toArray
                    |> makeEntity }
// [/snippet]

// [snippet:Usage]
namespace Sample

open System
open System.Data.Common
open XMicroOrm

type Customer =
    { CustomerID    : int;
      PersonID      : int option;
      StoreID       : int option;
      TerritoryID   : int option;
      AccountNumber : string;
      rowguid       : Guid;
      ModifiedDate  : DateTime }

module Program =
    let provider = @"System.Data.SqlClient"
    let cnnstr = @"Data Source=(LocalDB)\v11.0;Integrated Security=True;"
               + @"AttachDbFilename=D:\AdventureWorks2012_Data.mdf"

    let main _ =
        let factory = DbProviderFactories.GetFactory(provider)
        use cnn = factory.CreateConnection(cnnstr)
        cnn.Open()

        @"SELECT * FROM Sales.Customer"
        |> DbEntity.read<Customer> cnn
        |> Seq.take 10
        |> Seq.iter (printfn "%A")
// [/snippet]