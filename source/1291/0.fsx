
type E = {Node1:string; Node2:string; Weight:int} 
type G = E list

//one of the nodes should be in the set and the other not
//returns the node not in the set
//O(log n)
let hasOneNodeOf (e:E) (nodeSet:Set<string>) =
    if   nodeSet.Contains e.Node1 && not( nodeSet.Contains e.Node2) then Some e.Node2
    elif nodeSet.Contains e.Node2 && not( nodeSet.Contains e.Node1) then Some e.Node1
    else None

//finds the mininum weight edge from a set of nodes and a list of available edges
//the edges must be sorted in increasing order of weight
//retruns the minium edge, the new node, and the remaining edges (order is preserved)
//O(n log n) worst case - amortized complextity should be much better - O(log n)?
let minEdge nodeSet sortedEdges =
    let rec minEdge' vs esDone esLeft = //recursive inner function that does the work
        match esLeft with
        | [] -> None
        | e::rest -> 
            match nodeSet |> hasOneNodeOf e with
            | Some n -> Some(e, n, (esDone |> List.rev) @ rest)
            | None   -> minEdge' vs (e::esDone) rest
    minEdge' nodeSet [] sortedEdges

//find the minimum spanning tree of a non-empty, connected graph
//based on Kruskal's algorithm
//O(E log E)
let mst (g:G) = 
    let sortedEdges = g |> List.sortBy (fun e -> e.Weight)  //list of available edges  O(n log n)
    let nodeSet     = set [sortedEdges.[0].Node1]           //starting node set

    let numNodes    =  //O(n log n)
        g 
        |> Seq.collect (fun e -> [e.Node1; e.Node2]) 
        |> Seq.distinct 
        |> Seq.length

    let rec mst' nodeSet nodeCount minEdges availableEdges =  //recursive inner function
        if nodeCount = numNodes then
            minEdges
        else
            match minEdge nodeSet availableEdges  with //O(n log n) worst case, O(log n) amortized
            | Some (e,n,rest) -> mst' (nodeSet.Add n) (nodeCount + 1) (e::minEdges) rest
            | None   -> failwith "not a connected graph"

    mst' nodeSet 1 [] sortedEdges

let g =
    [
        {Node1="a"; Node2="b"; Weight=4}
        {Node1="a"; Node2="c"; Weight=2}
        {Node1="a"; Node2="e"; Weight=3}
        {Node1="b"; Node2="d"; Weight=5}
        {Node1="c"; Node2="d"; Weight=1}
        {Node1="c"; Node2="e"; Weight=6}
        {Node1="c"; Node2="f"; Weight=3}
        {Node1="d"; Node2="f"; Weight=6}
        {Node1="e"; Node2="f"; Weight=2}
    ]

let minSpanTree = mst g