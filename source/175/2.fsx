type Tup<'a, 'b> =
  struct 
    val Item1 : 'a
    val Item2 : 'b
    
    new(item1, item2) = { 
      Item1 = item1
      Item2 = item2
    }
  end

type Triple<'a, 'b, 'c> =
  struct 
    val Item1 : 'a
    val Item2 : 'b
    val Item3 : 'c
    
    new(item1, item2, item3) = { 
      Item1 = item1
      Item2 = item2
      Item3 = item3
    }
  end

type Quad<'a, 'b, 'c, 'd> =
  struct 
    val Item1 : 'a
    val Item2 : 'b
    val Item3 : 'c
    val Item4 : 'd
    
    new(item1, item2, item3, item4) = { 
      Item1 = item1
      Item2 = item2
      Item3 = item3
      Item4 = item4
    }
  end

let stup a b = Tup(a, b)
let striple a b c = Triple(a, b, c)
let squad a b c d = Quad(a, b, c, d)

//Usage
stup 1 2
striple 1 2 3
squad 1 2 3 4