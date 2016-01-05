type Node = Node of Node * Node | Leaf of int
let rec traverse node =
    seq { match node with Node (l, r) -> yield! traverse l; yield! traverse r | Leaf d -> yield d }
let compare l r = Seq.forall2 (=) (traverse l) (traverse r)

traverse (Node (Node (Node (Leaf 4, Leaf 1), Leaf 5), Leaf 6));;
// val it : seq<int> = seq [4; 1; 5; 6]
compare (Node (Node (Node (Leaf 4, Leaf 1), Leaf 5), Leaf 6)) (Node (Leaf 4, Node (Leaf 1, Node (Leaf 5, Leaf 6))));;
// val it : bool = true