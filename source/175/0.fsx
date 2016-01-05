type Tup<'a, 'b> =
  struct 
    val mutable Item1 : 'a
    val mutable Item2 : 'b
    
    new(item1, item2) = { 
      Item1 = item1
      Item2 = item2
    }
  end

let stup a b = Tup(a, b)

stup 1 2