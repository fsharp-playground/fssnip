// [snippet:Interop]
[<AutoOpen>]
module Interop =

    let (===) a b = obj.ReferenceEquals(a, b)
    let (<=>) a b = not (a === b)
    let inline isNull value = value === null
    let inline nil<'T> = Unchecked.defaultof<'T>
    let inline safeUnbox value = if isNull value then nil else unbox value
    let (|Null|_|) value = if isNull value then Some() else None
// [/snippet]

// [snippet:Example]
type Foo() = class end

type Test() =
    member this.AcceptFoo(foo:Foo) = //passed from C#
        if isNull foo then nullArg "foo"
        else ()

    member this.AcceptFoo2(foo:Foo) = //passed from C#
        match foo with
        | Null -> nullArg "foo"
        | _ -> ()

    member this.AcceptBoxedFoo(boxedFoo:obj) =
        let foo : Foo = safeUnbox boxedFoo
        ()

    member this.ReturnFoo() : Foo = //returning to C#
        if (true) then new Foo()
        else nil
// [/snippet]