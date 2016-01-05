open System
open System.Text

// No more foo.ToString() waste

let Out x = x.ToString()

type Object with

    member this.Out : string =
        this |> Out

// No more foo.Append(bar) waste

let (++) (left : System.Text.StringBuilder) (right : 't) : System.Text.StringBuilder =
    left.Append right

// No more foo.Append(bar) |> ignore waste

let (+=) (left : System.Text.StringBuilder) (right : 't) : unit =
    left ++ right |> ignore

// Let's see what we are left with.

let source =
    let mutable x, y, a, b = (*[omit:(...)]*)(0, 0, 0, 0)(*[/omit]*)
    let foo, bar, baz, waz, zup, bla = (*[omit:(...)]*)("foo", "bar", "baz", "waz", "zup", "bla")(*[/omit]*)
    let s = new StringBuilder()
    // ...
    while x < y do
        if a > b then
            s += bla
        // ...
    // ...
    s
    ++ foo
    ++ bar
    ++ baz
    ++ waz
    ++ zup
    |> Out
