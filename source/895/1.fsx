  // Define an immutable stack
  type ImmutableStack<'T> =
    | Empty 
    | Stack of 'T * ImmutableStack<'T>

    member s.Push x = Stack(x, s)

    member s.Pop() = 
      match s with
      | Empty -> failwith "Underflow"
      | Stack(t,_) -> t

    member s.Top() = 
      match s with
      | Empty -> failwith "Contain no elements"
      | Stack(_,st) -> st

    member s.IEmpty = 
      match s with
      | Empty -> true
      | _ -> false

    member s.All = 
      let rec loop acc = function
      | Empty -> acc
      | Stack(t,st) -> loop (t::acc) st
      loop [] s

  // Return an ImmutableStack<'a> object
  let s = ImmutableStack.Empty
  
  // Return Stack (5,Empty)
  let s' = s.Push 5

  // Return Stack (4,Stack (5,Empty))
  let s'' = s'.Push 4

  // Print out a list [5; 4]
  let printAll = s''.All

