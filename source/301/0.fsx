// The example is copied from the official WebSharper documentation:
// http://www.websharper.com/samples/HelloWorld
namespace Samples

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html

module HelloWorld =

    [<JavaScript>]
    let Main () =
        let welcome = P [Text "Welcome"]
        Div [
            welcome
            Button [Text "Click Me!"]
            |>! OnClick (fun e args ->
                welcome.Text <- "Hello, world!")
        ]

type HelloWorldViewer[<JavaScript>]() =
    inherit Web.Control()

    [<JavaScript>]
    override this.Body = HelloWorld.Main () :> Html.IPagelet
