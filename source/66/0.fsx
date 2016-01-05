(*[omit:(Triple Definition)]*)
type triple(x: float, y: float, z: float) =
  member o.X = x
  member o.Y = y
  member o.Z = z
  
  new (x: int, y: int, z: int) = triple(float x, float y, float z)
  
  static member ( * ) (a, b: triple) = triple(a * b.X, a * b.Y, a * b.Z)
  static member ( * ) (a, b: triple) =
    let af = float a
    triple(af * b.X, af * b.Y, af * b.Z)
  static member ( * ) (a: triple, b) = triple(b * a.X, b * a.Y, b * a.Z)
  static member ( * ) (a: triple, b) =
    let bf = float b
    triple(bf * a.X, bf * a.Y, bf * a.Z)
  static member ( * ) (a: triple, b: triple) = a.X * b.X + a.Y * b.Y + a.Z * b.Z
  static member ( .* ) (a: triple, b: triple) = triple(a.X * b.X, a.Y * b.Y, a.Z * b.Z)
  static member ( * ) (a: float [,], b: triple) = triple(a.[0,0] * b.X + a.[0,1] * b.Y + a.[0,2] * b.Z, a.[1,0] * b.X + a.[1,1] * b.Y + a.[1,2] * b.Z, a.[2,0] * b.X + a.[2,1] * b.Y + a.[2,2] * b.Z)

  static member ( / ) (a, b: triple) = triple(a / b.X, a / b.Y, a / b.Z)
  static member ( / ) (a, b: triple) =
    let af = float a
    triple(af / b.X, af / b.Y, af / b.Z)
  static member ( / ) (a: triple, b) = triple(a.X / b, a.Y / b, a.Z / b)
  static member ( / ) (a: triple, b) =
    let bf = float b
    triple(a.X / bf, a.Y / bf, a.Z / bf)
  static member ( ./ ) (a: triple, b: triple) = triple(a.X / b.X, a.Y / b.Y, a.Z / b.Z)

  static member (+) (a, b: triple) = triple(a + b.X, a + b.Y, a + b.Z)
  static member (+) (a, b: triple) =
    let af = float a
    triple(af + b.X, af + b.Y, af + b.Z)
  static member (+) (a: triple, b) = triple(b + a.X, b + a.Y, b + a.Z)
  static member (+) (a: triple, b) =
    let bf = float b
    triple(bf + a.X, bf + a.Y, bf + a.Z)
  static member (+) (a: triple, b: triple) = triple(a.X + b.X, a.Y + b.Y, a.Z + b.Z)

  static member (-) (a, b: triple) = triple(a - b.X, a - b.Y, a - b.Z)
  static member (-) (a, b: triple) =
    let af = float a
    triple(af - b.X, af - b.Y, af - b.Z)
  static member (-) (a: triple, b) = triple(a.X - b, a.Y - b, a.Z - b)
  static member (-) (a: triple, b) =
    let bf = float b
    triple(a.X - bf, a.Y - bf, a.Z - bf)
  static member (-) (a: triple, b: triple) = triple(a.X - b.X, a.Y - b.Y, a.Z - b.Z)

  static member (.**) (a: triple, b) = triple(a.X**b, a.Y**b, a.Z**b)

  static member (~-) (a: triple) = triple(-a.X, -a.Y, -a.Z)

  static member toTriple (x: int, y, z) = triple(x, y, z)
  static member toTriple (x: int list) = triple(x.[0], x.[1], x.[2])
  static member get_Zero() = triple (0.,0.,0.)
  member o.toTuple = x, y, z
  member o.toList = [x;y;z]
  member o.toArray = [|x;y;z|]
  member o.norm = sqrt (o.X * o.X + o.Y * o.Y + o.Z * o.Z)

  member o.Item (idx: int) =
    match idx with
      | 0 -> x
      | 1 -> y
      | 2 -> z
      | _ -> sprintf "invalid index %d in triple" idx |> failwith
  
  override o.ToString() = sprintf "[%f; %f; %f]" x y z

  override o.Equals(ob : obj) =
    match ob with
    | :? triple as other -> other.X = o.X && other.Y = o.Y && other.Z = o.Z
    | _ -> false
  override o.GetHashCode() =
    let hash = 23.
    let hash = hash * 31. + o.X
    let hash = hash * 31. + o.Y
    int (hash * 31. + o.Z)
  
module Triple =
  let toArray (t: triple) = [| t.X; t.Y; t.Z |]
  let toList (t: triple) = [t.X; t.Y; t.Z]
  let toTuple (t: triple) = t.X, t.Y, t.Z
  let dot (a: triple) (b: triple) = a.X * b.X + a.Y * b.Y + a.Z * b.Z
  let outer (a: triple) (b: triple) = 
    array2D [|  [| a.X * b.X; a.X * b.Y; a.X * b.Z |];
                [| a.Y * b.X; a.Y * b.Y; a.Y * b.Z |];
                [| a.Z * b.X; a.Z * b.Y; a.Z * b.Z |] |]
  let norm (t: triple) = sqrt (t.X * t.X + t.Y * t.Y + t.Z * t.Z)
  let norm2 (t: triple) = t.X * t.X + t.Y * t.Y + t.Z * t.Z
  let cross (a: triple) (b: triple) = triple(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X)    
  let map2 (fn: float->float->float) (a: triple) (b: triple) = triple(fn a.X b.X, fn a.Y b.Y, fn a.Z b.Z)
  let Zero = triple(0., 0., 0.)
  let Ones = triple(1.,1.,1.)
  let create (n: float) = triple(n,n,n)
  let exists fn (el: triple) =
    if fn el.X then true
    elif fn el.Y then true
    elif fn el.Z then true
    else false
  let map (fn: float->float) (el: triple) = triple(fn el.X, fn el.Y, fn el.Z)
  let normalize (t: triple) = t / (norm t)
  let init (fn: int->float) = triple(fn 0, fn 1, fn 2)
(*[/omit]*)

let segmentSegment (p1A: triple, p1B: triple) (p2A: triple, p2B: triple) =
  let SMALL_NUM = 1e-10
  let u = p1B - p1A
  let v = p2B - p2A
  let w = p1A - p2A
  let a = Triple.dot u u
  let b = Triple.dot u v
  let c = Triple.dot v v
  let d = Triple.dot u w
  let e = Triple.dot v w
  let D = a * c - b * b
  let sN, sD, tN, tD =
    if D < SMALL_NUM then 0.0, 1.0, e, c
    else
      let sN = b * e - c * d
      let tN = a * e - b * d
      if sN < 0.0 then 0.0, D, e, c
      elif sN > D then D, D, e + b, c
      else sN, D, tN, D
  let tN, sN, sD =
    if tN < 0.0 then
      let sN, sD = if -d < 0.0 then 0.0, sD elif -d > a then sD, sD else -d, a
      0.0, sN, sD
    elif tN > tD then
      let sN, sD = if -d + b < 0.0 then 0., sD elif -d + b > a then sD, sD else -d + b, a
      tD, sN, sD
    else tN, sN, sD
  let sc = if System.Math.Abs sN < SMALL_NUM then 0.0 else sN / sD
  let tc = if System.Math.Abs tN < SMALL_NUM then 0.0 else tN / tD
  p1A + (sc * u), p2A + (tc * v)