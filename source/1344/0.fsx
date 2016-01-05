open System

// First attempt at a KMeans clustering.
// Doesn't have an epsilon diff check which would allow early exit from loop.
let KMeans2D maxIterations numClusters (data:(float*float) list) = 
    let rec getMin points cur = 
        let oldMinX,oldMinY = cur
        match points with
        | [] -> cur
        | (minX,minY) :: tail when minX<oldMinX && minY < oldMinY -> getMin tail (minX,minY)
        | (minX,minY) :: tail when minX>oldMinX && minY < oldMinY -> getMin tail (oldMinX,minY)
        | (minX,minY) :: tail when minX<oldMinX && minY > oldMinY -> getMin tail (minX,oldMinY)
        | (minX,minY) :: tail when minX>oldMinX && minY > oldMinY -> getMin tail (oldMinX,oldMinY)
        | (minX,minY) :: tail -> getMin tail (oldMinX,oldMinY)
    let rec getMax points cur = 
        let oldmaxX,oldmaxY = cur
        match points with
        | [] -> cur
        | (maxX,maxY) :: tail when maxX>oldmaxX && maxY > oldmaxY -> getMax tail (maxX,maxY)
        | (maxX,maxY) :: tail when maxX<oldmaxX && maxY > oldmaxY -> getMax tail (oldmaxX,maxY)
        | (maxX,maxY) :: tail when maxX>oldmaxX && maxY < oldmaxY -> getMax tail (maxX,oldmaxY)
        | (maxX,maxY) :: tail when maxX<oldmaxX && maxY < oldmaxY -> getMax tail (oldmaxX,oldmaxY)
        | (maxX,maxY) :: tail -> getMax tail (oldmaxX,oldmaxY)
            
    let minX,minY = getMin data (Double.MaxValue,Double.MaxValue)
    let maxX,maxY = getMax data (Double.MinValue,Double.MinValue)

    let partition means points = 
        let distance mean pt = let meanX,meanY=mean
                               let ptX,ptY=pt
                               (mean,Math.Sqrt((meanX-ptX)**2.0+(meanY-ptY)**2.0),(ptX,ptY))
        let minDistance means pt = let (mean,len,pt) = means 
                                                       |> List.map (fun mean -> (distance mean pt))
                                                       |> List.minBy (fun (mean,len,pt)->len)
                                   (mean,pt)
        let parts = List.map (fun pt-> minDistance means pt ) points
        let groups = Seq.ofList parts |> Seq.groupBy (fun (mean,point)->mean)
        Seq.map (fun (key,value)->(key,List.ofSeq (Seq.map (fun (x,y)->y) value))) groups|>List.ofSeq

    let recalcMeans partitions = 
        let getClusterMean (cluster:(float*float)list) = 
            (List.averageBy (fun (ptX,ptY)->ptX) cluster,List.averageBy (fun (ptX,ptY)->ptY) cluster)
        List.map (fun (oldMean,cluster) -> getClusterMean cluster) partitions

    let getInitialClusters() = 
        let rnd = new System.Random()
        let randMeans = List.init numClusters (fun _ -> (minX+((maxX-minX)*rnd.NextDouble()),minY+((maxY-minY)*rnd.NextDouble())) ) 
        partition randMeans data
                                        
    let rec impl iteration clusters = 
        if List.isEmpty clusters then []
        else
            match iteration with
            | 0 -> clusters
            | _ -> let means = recalcMeans clusters
                   let newClusters = partition means data
                   impl (iteration-1) newClusters

    impl maxIterations (getInitialClusters())