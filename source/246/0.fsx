let (@?) (a:'a) (b:unit -> 'a) : 'a = 
  if System.Object.ReferenceEquals(a :> obj, null) 
    then b()
    else a
    
type Foo() = 
  class 
  end

let x = Unchecked.defaultof<Foo>
let z = x @? (fun()->Foo())