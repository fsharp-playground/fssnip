type MyAction<'T> =
    abstract member Call : 'T -> unit

type MyType () =
    interface MyAction<unit> with
        member t.Call() = () // Compiler error

type MyType2 () =
    interface MyAction<int * int> with
        member t.Call(_, _) = () // Compiler error

// Both cases work when using double parentheses