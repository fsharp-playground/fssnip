let lookup k = List.tryFind (fst >> ((=) k))

(* Example *)

[('a', 1); ('b', 2); ('c', 3)] |> lookup 'c'