// Declare our shape type, we'll test out matches a bit by exposing a draw function that dislikes points.
type Shape =
| Circle of int
| Rectangle of int * int
| Polygon of (int * int) list
| Point of (int * int)


// Draw shape function, no points or other geometries allowed, what a strange language, but this is interesting stuff!
let draw shape = 
    match shape with
    | Circle radius ->
        printfn "Rendering circle of radius: %i" radius
    | Rectangle (width, height) ->
        printfn "Rendering a rectangle of w: %i, h: %i" width height
    | Polygon points ->
        printfn "The polygon is made of the following points: %A" points
    | _ ->
        printfn "An unknown geometry type has been passed into the basic draw function"

(*
Multiline comment, but there is a bug in Xamarin Studio that if I start writing one, 
my code below starts being highlighted in it as well lol, or if it touches this comment from here on and I undo it.
*)

[<EntryPoint>]
let main argv = 
    let a = Circle(10)
    let b = Rectangle(20, 20)
    let c = Polygon([(0, 0); (20, 0); (20, 20); (0, 20)])
    let d = Point(20, 0)

    // Pipe the output of the list iterator function to the draw function.
    [a; b; c; d] |> List.iter draw
    0 // return.