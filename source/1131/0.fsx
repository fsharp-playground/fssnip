type Key = int

type BTree = 
    | Node of Key list * BTree * BTree * BTree 
    | Empty

let rec findKey (targetKey:Key) (bTree:BTree) = 
    match bTree with
        | Node (keyList, lessTree, betweenTree, greaterTree) ->
            let minKey = List.min keyList
            let maxKey = List.max keyList
            
            let extractor item = item = targetKey;

            if targetKey < minKey then
                findKey targetKey lessTree

            else if targetKey > maxKey then
                findKey targetKey greaterTree

            else if (List.exists extractor keyList) then
                let foundElement = List.find extractor keyList
                Some(foundElement)
            
            else 
                findKey targetKey betweenTree

        | Empty -> None

let lessTree = Node(([-12; -3; 0], Empty, Empty, Empty))

let greaterTree = Node(([100; 200; 300], Empty, Empty, Empty))

let middleTree = Node(([3], Empty, Empty, Empty))

let tree = Node(([1; 2; 4], lessTree, middleTree, greaterTree))

let key = findKey 3 tree
