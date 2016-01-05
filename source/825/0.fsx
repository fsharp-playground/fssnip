type TracksideView(position, graphics) = 
    let camera = TargetCamera(position, Vector3(), graphics)
    member val Target : option<ITransform> = None with get, set

    interface IView with
        member o.Camera = upcast camera
        member o.Update() =
            match o.Target with 
            | Some(target) ->
                camera.Target <- target.Transform.Position
                camera.Update()
            | None -> ()