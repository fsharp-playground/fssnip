type States = NotRun = 0
            | Run    = 1

let getTemplatePictureName (state: States) =
  match state with
  | States.NotRun -> "square.png"
  | States.Run    -> "triangle.png"