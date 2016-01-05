type TracksideView(position, graphics) = 
    let camera = TargetCamera(position, Vector3(), graphics)
    member val Target : option<ITransform> = None with get, set

    interface IView with
        member o.Camera = upcast camera
        member o.Update() =
            o.Target |> Option.iter (fun target ->
                camera.Target <- target.Transform.Position
                camera.Update())