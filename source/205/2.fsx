module GrahamScan

type Point = { X : int; Y : int }

/// finds the points lying on the convex hull of the given set of points and 
/// returns those points in clockwise direction, starting at the point
/// with minimum y-value
/// Remarks: it's a more or less direct implementation of the algorithm named
/// after Ronald Graham that is explained on http://en.wikipedia.org/wiki/Graham_scan
let FindConvexHull (pts : Point seq) : Point seq =

    // it's nicer to work with lists so let's convert
    let ptl = List.ofSeq pts

    // to make something worthwhile we need at last two points
    if ptl.Length <= 2 then Seq.empty
    else

    // this is a helperfunction (explained in the wikipedia article) in which direction
    // 3 points "turn"
    let ccw (a : Point) (b : Point) (c : Point) =
        (b.X - a.X)*(c.Y - a.Y) - (b.Y - a.Y)*(c.X - a.X)

    // 1. Let's find the point with the minimum y-coordinate
    /// this is the comparision function for this
    let cmpPts (a : Point) (b : Point) =
        match a.Y.CompareTo(b.Y) with
        | 0 -> a.X.CompareTo(b.X)
        | _ as r -> r
    // and with it we can look for the mentioned point
    let sortedY = ptl |> List.sortWith cmpPts
    let org, rest = sortedY.Head, sortedY.Tail

    // 2. we have to sort the list in increasing order of the angle
    // that a point p makes with org and the x-axis
    let winkelCos (p : Point) =
        let dx = float (p.X - org.X)
        let dy = float (p.Y - org.Y)
        let l = System.Math.Sqrt(dx*dx + dy*dy)
        dx / l
    // and here we sort the list (we only sort the remainder without
    // org and prepend it afterwards to ward of 
    // any issue with "division by zero"
    let sortedW = org::(rest |> List.sortBy winkelCos)

    // here is the actual algorithm
    // it uses two lists
    // lastPts: every visited point is put but might
    //          be removed if the "turn direction"
    //          'turns' out to be wrong
    // nextPts: the points left to be checked
    //          as the algorithm progresses those
    //          points are moved to lastPts
    // so lastPts will contain the found points
    // on the convex hull at every step, but in
    // clockwise orientation (as we push in front
    // of the list)
    let rec scan (lastPts : Point list) (nextPts : Point list) =
        // we are done if there are no points left to check
        if nextPts.IsEmpty then lastPts
        else

        // if there are points left take the first one
        let c = nextPts.Head

        match lastPts with
        // if there are at least 2 points b,a in the visited points
        // and a,b,c is NOT a counterclockwise turn
        // we have to remove b from lastPoints and continue checking
        // backwards ...
        | b::a::_ when ccw a b c >= 0 -> scan (lastPts.Tail) nextPts
        // in every other case we can push c onto the visited
        // stack and continue
        | _ -> scan (c::lastPts) nextPts.Tail

    // run it
    sortedW |> scan [] |> Seq.ofList