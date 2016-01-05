open Unchecked
open System
open System.Web.Mvc
open System.Web.Mvc.Async
open System.Net

// Preserving stack trace when rethrowing exceptions
// http://weblogs.asp.net/fmarguerie/archive/2008/01/02/rethrowing-exceptions-and-preserving-the-full-call-stack-trace.aspx
// http://stackoverflow.com/questions/57383/in-c-how-can-i-rethrow-innerexception-without-losing-stack-trace
exception PreserveStackTraceWrapper of exn

//New base async controller. 
type AsyncWorkflowController() = 
    inherit AsyncController()

    override __.CreateActionInvoker() = 
        //In real-life applications  object expression for AsyncControllerActionInvoker can be pulled off into separate type/file.
        //See how F# object expressions smooth out sharp OOP corners. 
        // In C# it would require to create 3 extra classes that only have meaning in local context.
        upcast {   
            new AsyncControllerActionInvoker() with

                member __.GetControllerDescriptor(controllerContext) =
                    let controllerType = controllerContext.Controller.GetType()

                    upcast {
                        new ReflectedControllerDescriptor(controllerType) with 
                            member ctrlDesc.FindAction(controllerContext, actionName) =
                                //getting default sync implementation 
                                let forwarder = base.FindAction(controllerContext, actionName) :?> ReflectedActionDescriptor
                                
                                if (forwarder <> null && forwarder.MethodInfo.ReturnType = typeof<Async<ActionResult>>) then 
                                    let endAsync' = ref (defaultof<IAsyncResult -> Choice<ActionResult, exn>>)

                                    upcast {
                                        new AsyncActionDescriptor() with

                                            member actionDesc.ActionName = forwarder.ActionName
                                            member actionDesc.ControllerDescriptor = upcast ctrlDesc
                                            member actionDesc.GetParameters() = forwarder.GetParameters()

                                            member actionDesc.BeginExecute(controllerContext, parameters, callback, state) =
                                                let asyncWorkflow = 
                                                    forwarder.Execute(controllerContext, parameters) :?> Async<ActionResult>
                                                    |> Async.Catch
                                                let beginAsync, endAsync, _ = Async.AsBeginEnd(fun () -> asyncWorkflow)
                                                endAsync' := endAsync
                                                beginAsync((), callback, state)

                                            member actionDesc.EndExecute(asyncResult) =
                                                match endAsync'.Value(asyncResult) with
                                                    | Choice1Of2 value -> box value
                                                    | Choice2Of2 why -> raise <| PreserveStackTraceWrapper(why)

                                    } 
                                else 
                                    upcast forwarder 
                    } 

        }

//Sample Asynchronous Controller
type MainController() = 
    inherit AsyncWorkflowController()

    member this.Index() = this.View()

    member this.LengthAsync() = 
        async {
            let wc = new WebClient()
            let! html = wc.AsyncDownloadString(Uri("http://news.google.com"))
            //Constrain under current design that method has to return Async <ActionResult>
            return ContentResult(Content = string html.Length) :> ActionResult 
        }
