type MyAction<'T> =
    abstract member Call : 'T -> unit

type MyType () =
    interface MyAction<unit> with
        member t.Call () = () // Compiler error