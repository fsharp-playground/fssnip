(*
How to avoid delegation boilerplate in inheritance heavy OOP code while maintaining proper accessibility
like seen here https://github.com/fsharp/fsharp/blob/master/src/fsharp/FSharp.Core/seq.fs#L82
and mentioned in Expert F# 2.0 "Using Partially Implemented Types via Implementation Inheritance "

Quote from the book:
"If implementation inheritance is used, you should in many cases consider making all implementing 
classes private or hiding all implementing classes behind a signature. For example, the 
Microsoft.FSharp.Collections.Seq module provides many implementations of the seq<'T> interface 
but exposes no implementation inheritance."

Some explanation on why implementation inheritance should be avoided.
http://sharp-gamedev.blogspot.com/2011/07/why-inheritance-for-code-reuse-should.html

This snippet should remedy the boilerplate mentioned in the following link therefore making it worthwhile.
http://sharp-gamedev.blogspot.com/2011/08/is-it-worth-it.html 

In this example only the explicit interface implementation of the base class will have the delegation boilerplate.
We inherit from the base class with an object expression and override some members and then immediately upcast to 
the interface.
This way we also simulate a protected Prop setter since the interface only exposes the getter

It also demonstates the use of F# 3.0 autoproperties.
*)

type IGameScreen =
    abstract Prop : string
    abstract Update : unit -> string

[<AbstractClass>]
type GameScreenBase() =
    member val Prop = "GameScreen.Prop" with get, set
    abstract Update : unit -> string
    interface IGameScreen with
        member o.Prop = o.Prop
        member o.Update() = o.Update()

let loadingScreen () =
    { new GameScreenBase(Prop = "loadingScreen.Prop") with
        override o.Update() = "loadingScreen.Update"  } :> IGameScreen
        
type IMenuScreen =
    inherit IGameScreen
    abstract MenuMethod : unit -> string

[<AbstractClass>]
type MenuScreenBase() =
    inherit GameScreenBase(Prop = "MenuScreen.Prop")
    abstract MenuMethod : unit -> string
    default o.MenuMethod() = 
        o.Prop <- "MenuScreenBase.Prop"
        "MenuScreen.MenuMethod"
    interface IMenuScreen with
        member o.MenuMethod() = o.MenuMethod()

let pauseMenuScreen () =
    { new MenuScreenBase(Prop = "pauseMenuScreen.Prop") with
        //override o.MenuMethod() = 
        //    o.Prop <- "pauseMenuScreen.Prop"
        //    "pauseMenuScreen.MenuMethod"
        override o.Update() = "pauseMenuScreen.Update" } :> IMenuScreen

module Example =
    let gameScreens : IGameScreen[] = [| loadingScreen () ; pauseMenuScreen () |]

    let testGameScreens (screens : IGameScreen[]) =
        for gs in screens do
            gs.Prop |> printfn "%s"       
            gs.Update() |> printfn "%s"

    testGameScreens gameScreens

    let menuScreens : IMenuScreen[] = [| pauseMenuScreen () |]

    let testMenuScreens (screens : IMenuScreen[]) =
        for ms in screens do
            ms.Prop |> printfn "%s"       
            ms.Update() |> printfn "%s"
            ms.MenuMethod() |> printfn "%s"
            ms.Prop |> printfn "%s"

    testMenuScreens menuScreens