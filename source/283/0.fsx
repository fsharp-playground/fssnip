open System
open System.Net
open System.Web.Mvc

// [snippet:Asynchronous Utilities for ASP.MVC]
/// A computation builder that is almost the same as stnadard F# 'async'.
/// The differnece is that it takes an ASP.NET MVC 'AsyncManager' as an
/// argumnet and implements 'Run' opration, so that the workflow is 
/// automatically executed after it is created (using the AsyncManager)
type AsyncActionBuilder(asyncMgr:Async.AsyncManager) = 

  (*[omit:(Boilerplate code that exposes 'async' operations)]*)
  member x.Bind(v, f) = async.Bind(v, f)
  member x.Combine(a, b) = async.Combine(a, b)
  member x.Delay(f) = async.Delay(f)
  member x.Return(v) = async.Return(v)
  member x.For(s, f) = async.For(s, f)
  member x.ReturnFrom(a) = async.ReturnFrom(a)
  member x.TryFinally(a, b) = async.TryFinally(a, b)
  member x.TryWith(a, b) = async.TryWith(a, b)
  member x.Using(r, f) = async.Using(r, f)
  member x.While(c, f) = async.While(c, f)
  member x.Zero() = async.Zero() (*[/omit]*)

  /// Run the workflow automatically using ASP.NET AsyncManager
  member x.Run(workflow) = 
    // Specify that there is some pending computation running
    asyncMgr.OutstandingOperations.Increment() |> ignore
    async { // Run the asynchronous workflow 
            let! res = workflow
            // Store the result of the workflow, so that it 
            // is passed as an argument to 'Completed' method.
            asyncMgr.Parameters.["result"] <- res
            // Notify the manager that the workflow has completed
            asyncMgr.OutstandingOperations.Decrement() |> ignore }
    |> Async.Start
     

/// An F# specific asynchronous controller that provides 
/// member 'AsyncAction' as a simple way of creating 
/// asynchronous actions (hiding 'AsyncManager').
type FSharpAsyncController() = 
  inherit AsyncController()
  member x.AsyncAction = 
    // Create new asynchronous builder using the current AsyncManager
    new AsyncActionBuilder(x.AsyncManager)
// [/snippet]

// [snippet:Sample Asynchronous Controller]
[<HandleError>]
type MainController() =
  inherit FSharpAsyncController()

  // Standard synchronous action that just renders view
  member x.Index() = x.View()

  // Asynchronous action that uses F# asynchronous workflows
  // to download a web page and then returns the length (in bytes)
  member x.LengthAsync(url:string) = x.AsyncAction {
    let wc = new WebClient()
    let! html = wc.AsyncDownloadString(url)
    return html.Length }

  // Called after the completion of workflow created by 'LengthAsync'
  // (the result of the workflow is passed as parameter named 'result')
  member x.HelloCompleted(result:int) =
    // Pass the result to the View
    x.ViewData.Model <- result
    x.View()
// [/snippet]