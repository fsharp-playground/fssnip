module NullRef =
  
  open System

  [<AllowNullLiteral>]
  type T<'a>(value:'a) =
    member x.Value = value

  let inline private isNull a = 
    Object.ReferenceEquals(box a, null)

  let create (value:'a) =
    new T<_>(value)

  let (@?) (a:T<'a>) (b:'a Lazy) = 
    if isNull a || isNull a.Value then 
      
      if isNull b then 
        failwith "Lazy object was null"

      let value = b.Value

      if isNull value then
        failwith "Lazy value was null"

      value

    else
      a.Value

open NullRef

type Foo() = class end

let z = create (Unchecked.defaultof<Foo>)
let d = z @? lazy Foo()