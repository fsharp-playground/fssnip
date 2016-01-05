module StructTuple =

  type Pair<'a, 'b> =
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

  let spair a b = Pair(a, b)
  let striple a b c = Triple(a, b, c)
  let squad a b c d = Quad(a, b, c, d)

  let spair_fst (pair:Pair<_, _>) = pair.Item1
  let spair_snd (pair:Pair<_, _>) = pair.Item2

  let spair_ref_fst (pair:Pair<_, _> byref) = pair.Item1
  let spair_ref_snd (pair:Pair<_, _> byref) = pair.Item2

  let striple_fst (triple:Triple<_, _, _>) = triple.Item1
  let striple_snd (triple:Triple<_, _, _>) = triple.Item2
  let striple_trd (triple:Triple<_, _, _>) = triple.Item3

  let striple_ref_fst (triple:Triple<_, _, _> byref) = triple.Item1
  let striple_ref_snd (triple:Triple<_, _, _> byref) = triple.Item2
  let striple_ref_trd (triple:Triple<_, _, _> byref) = triple.Item3

  let squad_fst (quad:Quad<_, _, _, _>) = quad.Item1
  let squad_snd (quad:Quad<_, _, _, _>) = quad.Item2
  let squad_trd (quad:Quad<_, _, _, _>) = quad.Item3
  let squad_fth (quad:Quad<_, _, _, _>) = quad.Item4

  let squad_ref_fst (quad:Quad<_, _, _, _> byref) = quad.Item1
  let squad_ref_snd (quad:Quad<_, _, _, _> byref) = quad.Item2
  let squad_ref_trd (quad:Quad<_, _, _, _> byref) = quad.Item3
  let squad_ref_fth (quad:Quad<_, _, _, _> byref) = quad.Item4