let (@?) (a:'a) (b:'a Lazy) : 'a = 
  if System.Object.ReferenceEquals(box a, null) 
    then b.Value
    else a
    
type Foo() = 
  class 
  end

let x = Unchecked.defaultof<Foo>
let z = x @? lazy Foo()