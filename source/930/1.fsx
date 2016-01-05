#r "System.Data.Services.Client"
#r "FSharp.Data.TypeProviders"

open Microsoft.FSharp.Data.TypeProviders

type stackOverflowData = 
    ODataService<"http://data.stackexchange.com/stackoverflow/atom">
let context = stackOverflowData.GetDataContext()

query { 
    for post in context.Posts do
    where (post.Tags.Contains "<f#>")
    sortByDescending post.CreationDate.Value
    take 10
    select post
}
|> Seq.map (fun post -> sprintf "%O %O %s" post.CreationDate post.AnswerCount post.Title)
|> Seq.toArray