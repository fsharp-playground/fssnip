// this generic tree will be used both
// for input and output
type Tree<'a> =
    | Branch of 'a * Tree<'a> list
    | Leaf of 'a

let geography = 
    Branch("Earth", [
        Branch("Brazil", [ 
            Leaf "Sao Paulo"; ]);
        Branch("USA", [ 
            Branch ("Virginia", []);
            Branch ("Texas", [
                Branch ("Houston", [ 
                    Leaf "Galleria" ]) ]) ]) ])

let population = [
    ("Galleria", 500);
    ("Virginia", 1000); 
    ("Sao Paulo", 2000)]
    
let lookupRegion k = List.tryFind (fst >> ((=) k))

let rec mergePopulationWithGeography geoData popData =
    // if we are dealing with large data-sets, then calling 
    // this lookup from each node may be too expensive
    let lookupRegionValue k =
        match popData |> lookupRegion k with
        | None -> 0
        | Some(_, x) -> x
    let sumUp items = 
        match items with
        | Leaf (k,v) -> v
        | Branch ((k,v), []) -> v
        | Branch ((k,v), children) -> v
    match geoData with
    | Leaf x -> Leaf (x, lookupRegionValue x)
    | Branch (x, []) -> Branch ((x, lookupRegionValue x), [])
    | Branch (x, children) ->
        // merge the descendents recursively all the way down
        let mappedChildren = children |> List.map (fun y -> popData |> mergePopulationWithGeography y)
        // only sum up the immediate descendants (Note: given that they
        // have already been merged "all the way down", then they will have
        // the data we need to calculate the correct sum)
        let sum = mappedChildren |> List.fold (fun acc t -> acc + sumUp t) 0
        Branch ((x, sum), mappedChildren)
