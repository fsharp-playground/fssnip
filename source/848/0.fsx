open System
open System.Runtime.Serialization

type SerializationInfo with
    member s.Get<'T> ?param =
        let param = typeof<'T>.FullName + "-" + defaultArg param ""
        s.GetValue(param, typeof<'T>) :?> 'T
    member s.Set<'T> (value : 'T, ?param) =
        let param = typeof<'T>.FullName + "-" + defaultArg param ""
        s.AddValue(param, value)

// example

[<Serializable>]
type Test(msg : string, inner : exn, foo : int, bar : int) =
    inherit Exception(msg, inner)

    new (s : SerializationInfo, _ : StreamingContext) =
        Test(s.Get(), s.Get(), s.Get "foo", s.Get "bar")

    member __.Foo = foo
    member __.Bar = bar

    interface ISerializable with
        member __.GetObjectData(s : SerializationInfo, _ : StreamingContext) =
            s.Set msg
            s.Set inner
            s.Set (foo, "foo")
            s.Set (bar, "bar")