open System.IO
open System.Reflection
open System.Runtime.Serialization.Formatters.Binary


let internal clone (e : #exn) =
    let bf = new BinaryFormatter()
    use m = new MemoryStream()
    bf.Serialize(m, e)
    m.Position <- 0L
    bf.Deserialize m :?> exn


let internal remoteStackTraceString =
    typeof<System.Exception>
        .GetField("_remoteStackTraceString", BindingFlags.Instance ||| BindingFlags.NonPublic)

let inline reraise' (e : #exn) =
    // clone the exception to avoid mutation side-effects
    let e' = clone e
    remoteStackTraceString.SetValue(e', e'.StackTrace + "\r\n")
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