type IGameScreenVirtuals = 
    abstract Update : unit -> string
     
type IGameScreen =
    abstract Prop : string
    abstract Virtuals : IGameScreenVirtuals

                 
let createGameScreenBase () =
    let prop = ref "GameScreen.Prop"
    (fun virtuals ->
        { new IGameScreen with
            member o.Prop = !prop
            member o.Virtuals = virtuals }), fun p -> prop := p
     
let createLoadingScreen () =
    let createBase, setProp = createGameScreenBase ()
    setProp "LoadingScreen.Prop"     
    createBase 
        { new IGameScreenVirtuals with
            member o.Update() = "LoadingScreen.Update" }
    

type IMenuScreenVirtuals = 
    abstract OnCancel : unit -> string
     
type IMenuScreen =
    abstract GameScreen : IGameScreen
    abstract Virtuals : IMenuScreenVirtuals
     
let createMenuScreenBase ()  =
    let createGameBase, setProp = createGameScreenBase ()
    setProp "MenuScreen.Prop"
    (fun gameMethods ->
        { new IMenuScreen with
            member o.GameScreen = createGameBase gameMethods
            member o.Virtuals = 
                { new IMenuScreenVirtuals with
                    member o.OnCancel() = "MenuScreen.OnCancel" } }), setProp
     
let createPauseMenuScreen () =
    let createMenuBase, setProp = createMenuScreenBase ()
    setProp "PauseMenuScreen.Prop"
    createMenuBase 
        { new IGameScreenVirtuals with
            member o.Update() = "pauseMenuScreen.Update" }
     
module Example =
    let gameScreens : IGameScreen[] = [| createLoadingScreen (); (createPauseMenuScreen ()).GameScreen|]
     
    let testGameScreens (screens : IGameScreen[]) =
        for gs in screens do
            gs.Prop |> printfn "%s"
            gs.Virtuals.Update () |> printfn "%s"
     
    testGameScreens gameScreens
     
    let menuScreens : IMenuScreen[] = [| createPauseMenuScreen () |]
     
    let testMenuScreens (screens: IMenuScreen[]) =
        for ms in screens do
            ms.GameScreen.Prop |> printfn "%s"
            ms.GameScreen.Virtuals.Update () |> printfn "%s"
            ms.Virtuals.OnCancel () |> printfn "%s"
     
    testMenuScreens menuScreens

