(*
    Types
*)
type Vector = { X:single; Y:single } with
    static member ( + ) ({X=x;Y=y}, {X=x';Y=y'}) = {X=x+x';Y=y+y'}
    static member ( - ) ({X=x;Y=y}, {X=x';Y=y'}) = {X=x-x';Y=y-y'}
    static member ( * ) ({X=x;Y=y}, s)           = {X=s*x;Y=s*y}
    static member ( * ) (s, {X=x;Y=y})           = {X=s*x;Y=s*y}
type Frame = { Origin:Vector; First:Vector; Second:Vector }
type Pict = Frame -> unit

let coordMap f = fun v -> f.Origin + (v.X * f.First + v.Y * f.Second)

(*
    Picture transformations and combinators
*)
let transform o c1 c2 (p:Pict) : Pict =
    let p' f =
        let map = coordMap f
        let o' = map o
        p { Origin=o'; First=(map c1) - o'; Second=(map c2) - o' }
    p'

let flipVert p = transform {X=0.f;Y=1.f} {X=1.f;Y=1.f} {X=0.f;Y=0.f} p
let flipHoriz p = transform {X=1.f;Y=0.f} {X=0.f;Y=0.f} {X=1.f;Y=1.f} p

let shrink p percent =
    let d = (single percent)/200.f
    transform {X=d;Y=d} {X=1.f-d;Y=d} {X=d;Y=1.f-d} p

let beside p1 p2 : Pict =
    let left = transform {X=0.f;Y=0.f} {X=0.5f;Y=0.f} {X=0.f;Y=1.f} p1
    let right = transform {X=0.5f;Y=0.f} {X=1.f;Y=0.f} {X=0.5f;Y=1.f} p2
    fun f -> left f; right f

let above p1 p2 : Pict =
    let low = transform {X=0.f;Y=0.f} {X=1.f;Y=0.f} {X=0.f;Y=0.5f} p1
    let high = transform {X=0.f;Y=0.5f} {X=1.f;Y=0.5f} {X=0.f;Y=1.f} p2
    fun f -> low f; high f

let rec rightSplit p n =
    if n = 0 then p
    else
        let smaller = rightSplit p (n-1)
        beside p (above smaller smaller)

let rec upSplit p n =
    if n = 0 then p
    else
        let smaller = upSplit p (n-1)
        above p (beside smaller smaller)

let rec cornerSplit p n =
    if n = 0 then p
    else
        let up = upSplit p (n-1)
        let right = rightSplit p (n-1)
        let topLeft = beside up up
        let bottomRight = above right right
        let corner = cornerSplit p (n-1)
        beside (above p topLeft) (above bottomRight corner)

let four p = let top = (beside (flipHoriz p) p) in above (flipVert top) top

let escher p n = four (cornerSplit p n)

(*
   Output
*)
open System.Windows.Forms
open System.Drawing

let mutable form:Form = null

let drawLine (v1,v2) =
    form.CreateGraphics().DrawLine(Pens.Red, v1.X, v1.Y, v2.X, v2.Y)

let paint p =
    form.Refresh()
    (flipVert p) { Origin={X=0.f;Y=0.f}; First={X=500.f;Y=0.f}; Second={X=0.f;Y=500.f} }

let setup() =
    form <- new Form(Size=Size(517,539))
    form.Show()

(*
    Primitive figures
*)
let makePict segs : Pict =
    fun f ->
        let map = coordMap f
        let transform (x1,y1,x2,y2) = map {X=x1;Y=y1}, map {X=x2;Y=y2}
        List.iter (drawLine << transform) segs
let xFig = [
    (0.f, 0.f, 1.f, 1.f)
    (1.f, 0.f, 0.f, 1.f)]
let yFig = [
    (0.5f, 0.f, 0.5f, 0.5f)
    (0.5f, 0.5f, 1.f, 1.f)
    (0.5f, 0.5f, 0.f, 1.f)]
let zFig = [
    (0.f, 1.f, 1.f, 1.f)
    (1.f, 1.f, 0.f, 0.f)
    (0.f, 0.f, 1.f, 0.f)]

(*
    Some test pictures
*)
let xPict = makePict xFig
let yPict = makePict yFig
let zPict = makePict zFig

(*  Usage:

> setup();;
> paint (four (shrink zPict 10));;
> let tile = (above (flipVert yPict) yPict);;
> paint (escher tile 4);;
etc.
*)
