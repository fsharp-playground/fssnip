type Lazy<'T> = 
  | Lazy of 'T ref
  static member RefValue(Lazy a) = a
  member l.Force() = (Lazy.RefValue l) |> (!)

let lazyValue = Lazy (ref 10)

let res = lazyValue.Force()

