[<AbstractClass>]
type GameScreen(?prop : ref<string>) =
    member val Prop = !(defaultArg prop (ref "GameScreen.Prop"))
    abstract Update : unit -> string

let loadingScreen () =
    { new GameScreen(prop = ref "LoadingScreen.Prop") with
        override o.Update() = "loadingScreen.Update"  }

[<AbstractClass>]
type MenuScreen(?prop : ref<string>) =
    inherit GameScreen(prop = defaultArg prop (ref "MenuScreen.Prop"))
    abstract OnCancel : unit -> string
    default o.OnCancel() = "MenuScreen.OnCancel"

let pauseMenuScreen () =
    { new MenuScreen(prop = ref "PauseMenuScreen.Prop") with
        //override o.OnCancel() = "pauseMenuScreen.OnCancel"
        override o.Update() = "PauseMenuScreen.Update" }

module Example =
    let gameScreens : GameScreen[] = [| loadingScreen () ; pauseMenuScreen () |]

    let testGameScreens (screens : GameScreen[]) =
        for gs in screens do
            gs.Prop |> printfn "%s"       
            gs.Update() |> printfn "%s"

    testGameScreens gameScreens

    let menuScreens : MenuScreen[] = [| pauseMenuScreen () |]

    let testMenuScreens (screens : MenuScreen[]) =
        for ms in screens do
            ms.Prop |> printfn "%s"       
            ms.Update() |> printfn "%s"
            ms.OnCancel() |> printfn "%s"

    testMenuScreens menuScreens