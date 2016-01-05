#r "System.Net.Http"

open System
open System.Diagnostics.Contracts
open System.Net
open System.Net.Http
open System.Reflection
open System.Web.Http
open System.Web.Http.Controllers

type AsyncApiActionInvoker() =
    inherit Controllers.ApiControllerActionInvoker()

    let (|Async|_|) (ty: Type) =
        if ty.IsGenericType && ty.GetGenericTypeDefinition() = typedefof<Async<_>> then
            Some (ty.GetGenericArguments().[0])
        else None

    static let asTaskMethod = typeof<AsyncApiActionInvoker>.GetMethod("StartAsTask", BindingFlags.NonPublic ||| BindingFlags.Static)
    static let voidResultConverter = VoidResultConverter()

    static member internal GetResultConverter<'T>(actionContext: HttpActionContext) : IActionResultConverter =
        let instanceType = typeof<'T>
        if instanceType <> null && instanceType.IsGenericParameter then
            raise <| InvalidOperationException()

        if instanceType = null || typeof<HttpResponseMessage>.IsAssignableFrom instanceType then
            actionContext.ActionDescriptor.ResultConverter
        elif instanceType = typeof<unit> then
            voidResultConverter :> _
        else new ValueResultConverter<'T>() :> _

    static member internal StartAsTask<'T>(actionContext: HttpActionContext, cancellationToken) =
        let resultConverter = AsyncApiActionInvoker.GetResultConverter<'T>(actionContext)
        let computation = async {
            let task =
                actionContext.ActionDescriptor.ExecuteAsync(
                    actionContext.ControllerContext,
                    actionContext.ActionArguments,
                    cancellationToken)
            let! result = Async.AwaitTask task
            let! (value: 'T) = unbox result
            return resultConverter.Convert(actionContext.ControllerContext, value) }
        Async.StartAsTask(computation, cancellationToken = cancellationToken)

    override this.InvokeActionAsync(actionContext, cancellationToken) =
        if actionContext = null then
            raise <| ArgumentNullException("actionContext")

        match actionContext.ActionDescriptor.ReturnType with
        | Async resultType ->
            let specialized = asTaskMethod.MakeGenericMethod(resultType)
            downcast specialized.Invoke(null, [| actionContext; cancellationToken |])
        | _ -> base.InvokeActionAsync(actionContext, cancellationToken)
