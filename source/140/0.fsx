[<RequireQualifiedAccess>]
module LinkedList =   
   open System.Collections.Generic
   let empty<'a> = LinkedList<'a>()
   let find (f:'a->bool) (xs:LinkedList<'a>) =
      let node = ref xs.First
      while (!node <> null && not <| f((!node).Value)) do 
         node := (!node).Next
      !node
   let findi (f:int->'a->bool) (xs:LinkedList<'a>) =
      let node = ref xs.First
      let i = ref 0
      while (!node <> null && not <| f (!i) (!node).Value) do 
         incr i; node := (!node).Next
      if !node = null then -1 else !i