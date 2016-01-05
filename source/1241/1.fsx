open System.Reflection


let internal clone (e : #exn) =
    let bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
    use m = new System.IO.MemoryStream()
    bf.Serialize(m, e)
    m.Position <- 0L
    bf.Deserialize m :?> exn

let internal remoteStackTraceField =
    let getField name = typeof<System.Exception>.GetField(name, BindingFlags.Instance ||| BindingFlags.NonPublic)
    match getField "remote_stack_trace" with
    | null ->
        match getField "_remoteStackTraceString" with
        | null -> failwith "a piece of unreliable code has just failed."
        | f -> f
    | f -> f

let inline reraise' (e : #exn) =
    // clone the exception to avoid mutation side-effects
    let e' = clone e
    remoteStackTraceField.SetValue(e', e'.StackTrace + System.Environment.NewLine)
    raise e'


// examples

type Test =
    static member Factorial (n : int) =
        if n = 0 then failwith "boom!"
        else
            n * Test.Factorial(n-1)

try Test.Factorial 42
with e -> reraise' e


// reraise in workflows

async {
    try return Test.Factorial 42
    with e -> return reraise' e
} |> Async.RunSynchronously


// unpacking TargetInvocationExceptions without losing the inner stacktrace

type MethodInfo with
    member m.GuardedInvoke(instance : obj, parameters : obj []) =
        try m.Invoke(instance, parameters)
        with :? TargetInvocationException as e when e.InnerException <> null ->
            reraise' e.InnerException

typeof<Test>.GetMethod("Factorial").GuardedInvoke(null, [| 42 :> obj |])