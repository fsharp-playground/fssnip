namespace AdaBooster

    type LabelType = 
        | NEG = -1
        | POS = 1

    type AdaBooster(trainingSamples : (obj * LabelType) [], weakClassifiers : (obj -> LabelType) [], weakClassifierMaxCount : int) =
        
        let trainingSamplesCount = trainingSamples.Length
        let weakClassifierCount = weakClassifiers.Length

        let weightMatrix = Array2D.create trainingSamplesCount weakClassifierMaxCount 1.0

        let scoutingMatrix = Array2D.create trainingSamplesCount weakClassifierCount 0

        let mutable strongClassifier: (int * float) list = []

        let initializeScoutingMatrix = 
            for j in 0 .. weakClassifiers.Length - 1 do
                for i in 0 .. trainingSamplesCount - 1 do
                    let (sample, label) = trainingSamples.[i]
                    scoutingMatrix.[i, j] <- 
                        if ( weakClassifiers.[j] sample ) <> label then 1 else 0

        let calculateError weakClassifierIndex weights = 
            weights     
            |> List.mapi (fun i wt -> float scoutingMatrix.[i, weakClassifierIndex] * wt)
            |> List.sum

        let selectBestWeakClassifier weights weakClassifierIndices = 
            weakClassifierIndices
            |> List.map (fun weakClassifierIndex -> (weakClassifierIndex, calculateError weakClassifierIndex weights))
            |> List.minBy (fun (x, y) -> y)

        let rec train m weights weakClassifierIndices =

            if m >= weakClassifierMaxCount then []
            else

                let nextM = m + 1 
                let (bestWeakClassifierIndex, classifierWeight) = selectBestWeakClassifier weights weakClassifierIndices
                let error = classifierWeight / List.sum weights
                let weakClassifierCoefficient = 0.5 * log ((1.0 - error) / error)
                let wm1 = sqrt(( 1.0 - error) / error)

                let newWeights =
                    weights 
                    |> List.mapi ( fun i wm -> if scoutingMatrix.[ i, bestWeakClassifierIndex]  = 1 then wm * wm1 else wm / wm1)

                for i in 0 .. (trainingSamplesCount - 1) do 
                    weightMatrix.[i, m] <- weights.[i]

                let weakClassifiersToScanNext = 
                    weakClassifierIndices 
                    |> List.filter (fun index -> index <> bestWeakClassifierIndex)

                (bestWeakClassifierIndex, weakClassifierCoefficient) :: train nextM newWeights weakClassifiersToScanNext

        member this.Predict sample  = 
            strongClassifier 
            |> List.sumBy (fun (weakClassifierIndex,  weakClassifierCoefficient) ->  weakClassifierCoefficient * float ( weakClassifiers.[weakClassifierIndex] sample) )
            |> (fun x -> if x > 0.0 then (x, LabelType.POS) else (x,LabelType.NEG))

        member this.Train  = 
            let initialWeights = List.init trainingSamplesCount (fun x -> 1.0)
            
            strongClassifier <- train 0 initialWeights [0 .. (weakClassifierCount - 1)]
            strongClassifier

    
        member this.ScoutingMatrix = 
            scoutingMatrix

        member this.WeightMatrix = 
            weightMatrix