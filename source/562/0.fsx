(*
For the SQLite specific libraries used in this snippet, see the following links:
http://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki
http://code.google.com/p/dblinq2007/downloads/list

For this example, an SQLite version of the Northwind Database was used; this file is included in the System.Data.SQLite download
*)

let pathToSQLiteDB = @"D:\Projects\Research\Data\northwindEF.db"

#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\System.Data.Linq.dll"

#r @"D:\Projects\Research\Libraries\FSharp.PowerPack.dll"
#r @"D:\Projects\Research\Libraries\FSharp.PowerPack.Linq.dll"

#r @"D:\Projects\Research\Libraries\DbLinq.dll"
#r @"D:\Projects\Research\Libraries\DbLinq.SQLite.dll"
#r @"D:\Projects\Research\Libraries\System.Data.SQLite.dll"

open System.Data.Linq
open System.Data.Linq.Mapping
open System.Data.SQLite

open Microsoft.FSharp.Linq
open Microsoft.FSharp.Linq.Query

[<Table(Name="Customers")>]
type Customer (customerId:string,companyName:string,contactName:string)=
    let mutable m_customerId = customerId
    let mutable m_companyName = companyName
    let mutable m_contactName = contactName

    new() = Customer(null, null, null)

    [<Column(IsPrimaryKey=true)>]
    member this.CustomerId
        with get() = m_customerId
        and set(value) = m_customerId <- value

    [<Column>]
    member this.CompanyName
        with get() = m_companyName
        and set(value) = m_companyName <- value

    [<Column>]
    member this.ContactName
        with get() = m_contactName
        and set(value) = m_contactName <- value

    override this.ToString() = System.String.Format("[{0}] {1}", m_customerId, m_companyName)

let connString = System.String.Format("Data Source={0};UTF8Encoding=True;Version=3", pathToSQLiteDB)
let conn = new SQLiteConnection(connString);
let db = new DataContext(conn)
let customers = db.GetTable<Customer>()

//query for company names
let companyNameListList = 
    seq {for i in customers do
                    yield i.CompanyName}
    |> Seq.toList


//find company with id "ALFKI"
let alfki =
    query <@ seq { for c in customers do
                        if c.CustomerId = "ALFKI" then
                            yield c}  @>
    |> Seq.head

//update name and save to DB
alfki.CompanyName <- "Alfreds Futterkiste (test)"
db.SubmitChanges()

//run the following code to restore the original name and save it to the DB
//alfki.CompanyName <- "Alfreds Futterkiste"
//db.SubmitChanges()

//release resources
//db.Dispose()
