// From Wikipedia: http://en.wikipedia.org/wiki/Disjoint-set_data_structure
// In computing, a disjoint-set data structure is a data structure that keeps track 
// of a set of elements partitioned into a number of disjoint (nonoverlapping) 
// subsets. A union-find algorithm is an algorithm that performs two useful 
// operations on such a data structure:

// Find: Determine which set a particular element is in. Also useful for 
//       determining if two elements are in the same set.
// Union: Combine or merge two sets into a single set.

// Implementation from: http://www.cs.princeton.edu/~rs/AlgsDS07/01UnionFind.pdf

/// Quick-find... Union is O(N), Flat Trees
/// Quick-union... Trees are tall, Find is O(N), Find requires union
/// Overall: O(MN) -- M union-find ops on a set of N objects
type QuickUnion (N) =
    //Parent index, id[i] is parent of i
    let mutable id : int[] = Array.init N (fun i -> i)

    let root i = 
        let mutable q = i
        while (q <> id.[q]) do q <- id.[q] 
        q

    member t.find(p, q) =
        root(p) = root(q)

    member t.unite(p, q) =
        let i = root(p)
        let j = root(q)
        id.[i] = j;

/// Weighted QuickUnion 
/// Now with logN union, lgN find
/// Overall: O(N+MLogN) -- M union-find ops on a set of N objects
type WeightedQuickUnion (N) =
    //Parent index, id[i] is parent of i
    let id : int[] = Array.init N (fun i -> i)
    //Number of elements rooted at i
    let sz : int[] = Array.create N 0
    
    let root i = 
        let mutable q = i
        while (q <> id.[q]) do q <- id.[q] 
        q

    member t.find(p, q) =
        root(p) = root(q)

    member t.unite(p, q) =
        let i = root(p)
        let j = root(q)
        if sz.[i] < sz.[j] then id.[i] <- j; sz.[j] <- sz.[j] + sz.[i]
        else id.[j] <- i; sz.[i] <- sz.[i] + sz.[j]

/// Weighted quick-union with path compression
/// Overall: O((M+N)lg*N) -- M union-find ops on a set of N objects
type QuickUWPC (N) =
    //Parent index, id[i] is parent of i
    let id : int[] = Array.init N (fun i -> i)
    //Number of elements rooted at i
    let sz : int[] = Array.create N 0
     
    let root i = 
        let mutable q = i
        while (q <> id.[q]) do 
            id.[q] <- id.[id.[q]]
            q <- id.[q] 
        q

    member t.find(p, q) =
        root(p) = root(q)

    member t.unite(p, q) =
        let i = root(p)
        let j = root(q)
        if sz.[i] < sz.[j] then id.[i] <- j; sz.[j] <- sz.[j] + sz.[i]
        else id.[j] <- i; sz.[i] <- sz.[i] + sz.[j]    