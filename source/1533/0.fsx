open System

// arr1 is always equal in size or smaller than arr2
let bestRotGos (f: 'a -> 'a -> float) (arr1: 'a []) (arr2: 'a []) =
    // Start Gosper's Hack Machinery Bootstrap
    let bitsConfig = Array.create arr1.Length Int32.MinValue
   
    let inline setBits v =
        let inline getBit x i = x &&& (1 <<< i) <> 0
        let mutable idx = 0
        for i = 0 to 31 do
            if getBit v i then
                bitsConfig.[idx] <- i
                idx <- idx + 1
 
    let inline nextGosper set =
        let c = set &&& -set
        let r = set + c
        r + (((r^^^set)/c)>>>2)
        
    let limit =
        let n = arr2.Length
        (1 <<< n)
 
    let mutable set =
        let k = arr1.Length
        (1 <<< k) - 1    
    // End Gosper's Hack Machinery Bootstrap

    // Pre-calculate sims and init placeholder variables
    let scores = Array2D.init arr1.Length arr2.Length 
                    (fun i j -> f arr1.[i] arr2.[j])    
                        
    let mutable maxScore = Double.NegativeInfinity
    let bestConfig = Array.create arr1.Length Int32.MinValue

    while (set < limit) do
        setBits set // Turn "set" bits into indices in "bitsConfig"

        // For each rotation
        for i = 0 to bitsConfig.Length - 1 do

            // calculate score and compare with previous best
            let mutable totalScore = 0.0
            for i = 0 to bitsConfig.Length - 1 do
                let tokenScore = scores.[i,bitsConfig.[i]]
                totalScore <- totalScore + tokenScore
            if totalScore > maxScore then
                // and replace if better
                maxScore <- totalScore
                Array.blit bitsConfig 0 
                           bestConfig 0 
                           bitsConfig.Length 

            // Rotate the array
            let firstElement = bitsConfig.[0]
            for i = 0 to bitsConfig.Length - 2 do
                bitsConfig.[i] <- bitsConfig.[i + 1]
            bitsConfig.[bitsConfig.Length - 1] <- firstElement
        
        set <- nextGosper set // Put the next combination in "set"
    bestConfig, maxScore
