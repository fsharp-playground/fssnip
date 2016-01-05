module Program

open Varak
open Varak.UI
open Varak.UI.GLTools
open System


/// Will be called after OpenGL setup but before rendering
let init (rWin : RenderWindow) =
    // Premultiplied alpha and y-flip are assumed in images.
    let texA = Texture.FromFilePA "myTex.png"
    let texB = Texture.FromFilePA "myOtherTex.png"
    let renderer = new RectRenderer()

    // A primitive viewer that just paints a texture over the whole view
    let makeTestViewer texture =
        { new IView with
            member v.Render _ = // Argument is view diagonal in isotropic coords
                renderer.Render(texture, Rect.unit, Mat.Id2d) }
        |> ViewArea

    // Dispose resources when a window is closed (OpenTK GameWindow feature)
    rWin.Unload.Add(fun _ ->
        dispose texA
        dispose texB
        dispose renderer
    )

    // The initializer returns a ViewPartition object that can subdivide the main window.
    // ViewPartitions are DU trees that can be used for window managers, split-screen etc.
    let leftRect, rightRect = Rect.unit.SplitHorizontal 0.5
    ViewDivision [leftRect, makeTestViewer texA; rightRect, makeTestViewer texB]


[<EntryPoint>]
let main argv =
    use main = new RenderWindow(init, 1920, 1080)
    main.Run() // You should now see a window split into two texturized areas.
    0