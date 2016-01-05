#r "System.Data.Services.Client"
#r "FSharp.Data.TypeProviders"

type nuget = Microsoft.FSharp.Data.TypeProviders.ODataService<"https://nuget.org/api/v2">
type Package = nuget.ServiceTypes.V2FeedPackage
let ctx = nuget.GetDataContext()

let packagesId = ["FSharpx.Http"; "FSharpx.Core"]

open System.Linq

let packages_throws_error() = // throws System.NotSupportedException: The method 'Contains' is not supported.
    query {
        for p in ctx.Packages do
        where ((packagesId.Contains(p.Id)) && p.IsAbsoluteLatestVersion && p.IsLatestVersion)
        select p
    }
    |> Seq.toList

// We implement it by folding ORs:

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns

let inList (membr: Expr<'a -> 'b>) (values: 'b list) : Expr<'a -> bool> =
    match membr with
    | Lambda (_, PropertyGet _) -> 
        match values with
        | [] -> <@ fun _ -> true @>
        | _ -> values |> Seq.map (fun v -> <@ fun a -> (%membr) a = v @>) |> Seq.reduce (fun a b -> <@ fun x -> (%a) x || (%b) x @>)
    | _ -> failwith "Expression has to be a member"

let packages = 
    query {
        for p in ctx.Packages do
        where (((%(inList <@ fun (x: Package) -> x.Id @> packagesId)) p) && p.IsAbsoluteLatestVersion && p.IsLatestVersion)
        select p
    }
    |> Seq.toList
