type Tree<'T> =
        |Branch of 'T * seq<Tree<'T>>
        |Leaf of 'T

let rec GenerateTreeFromRecursiveObject hasChildren generateChildren current =
    let generateTree = GenerateTreeFromRecursiveObject hasChildren generateChildren
    match hasChildren current with
    | true -> Branch(current, seq{for next in generateChildren current do yield (generateTree next)})
    | false -> Leaf(current)