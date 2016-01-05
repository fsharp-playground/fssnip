open System
open System.Net
open System.Threading.Tasks

/// Implements an extension method that overloads the standard
/// 'Bind' of the 'async' builder. The new overload awaits on 
/// a standard .NET task
type Microsoft.FSharp.Control.AsyncBuilder with
  member x.Bind(t:Task<'T>, f:'T -> Async<'R>) : Async<'R>  = 
    async.Bind(Async.AwaitTask t, f)

/// Now we can use let! keyword directly with .NET Tasks 
/// (without the need to use AwaitTask explicitly)
let download(url : string) =
    async {
        let client = new WebClient()
        let! html = client.DownloadStringTaskAsync(url)
        return html 
    }
