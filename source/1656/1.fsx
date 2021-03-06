let tower x y = 
  ( Fun.cone 
    |> Fun.color 0xff0000 
    |> Fun.scale (1.2, 1.2, 1.2) ) $ 
  ( Fun.cylinder 
    |> Fun.color 0xffff00
    |> Fun.scale (1.0, 3.0, 1.0) 
    |> Fun.move (0.0, -2.0, 0.0) )
  |> Fun.move (x, 2.0, y)

let wall rotation = 
  [ 0 .. 20 ] 
  |> List.map (fun i ->
      let scale = 
        if i%2=0 then 1.0 else 0.8
      let offs = (1.0-scale)/2.0
      Fun.cube 
      |> Fun.scale (1.0, scale, 1.0) 
      |> Fun.move (float i, -offs, 0.0)  )
  |> List.reduce ($)
  |> Fun.scale (0.25, 2.0, 0.5)
  |> Fun.move (-2.5, -0.5, -3.0)
  |> Fun.rotate (0.0, rotation, 0.0)

tower -3.0 -3.0 $ tower -3.0 3.0 $
tower 3.0 3.0 $ tower 3.0 -3.0 $
wall 0.0 $ wall 90.0 $ 
wall 180.0 $ wall 270.0 