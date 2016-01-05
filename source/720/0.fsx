(*
#r "System.Data.Entity.dll"
#r "FSharp.Data.TypeProviders.dll"
*)
open System
open Microsoft.FSharp.Data.TypeProviders

module Queries =
 
    [<Literal>] 
    let connectionstring = "Server=localhost;Initial Catalog=Northwind;Integrated Security=SSPI;MultipleActiveResultSets=true"
    
    //<interactive at once>
    type private EntityConnection = SqlEntityConnection<ConnectionString=connectionstring, Pluralize = true>
    
    let fetchCustomers = 
        let context = EntityConnection.GetDataContext()

        query { for customer in context.Customers do
                where (customer.Country = "Finland")
                select (customer.ContactName, customer.CompanyName) }
    //</interactive at once>

    let show = fetchCustomers |> Seq.iter (fun i -> Console.WriteLine("Customer: " + fst i + ", Company: " + snd i))

(* Output:

Customer: Pirkko Koskitalo, Company: Wartian Herkku
Customer: Matti Karttunen, Company: Wilman Kala

val show : unit = ()
*)
