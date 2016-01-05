module PRef = 
  type RO = RO
  type RW = RW

  type t<'a, 'b>(r:'a ref) = 
    member internal x.Ref = r

  let pref r = t<_, RW>(ref r)
  let pref_ref r = t<_, RW>(r)

  let inline (!!) (r:t<_, _>) = !r.Ref
  let get r = !!r

  let inline (=<) (r:t<_, RW>) v = r.Ref := v
  let set r v = r =< v

  let ro (r:t<_, _>) = t<_, RO>(r.Ref)

//FSI Example
open PRef
let x = pref 1

x =< 3
!!x // 3

let z = ro x
z =< 2 // fails, z is RO