open System

let getFitedData (slice : int16[] , outputsize : int, window : int, mixFractional : int) : int16[] =
    let mutable MXVal = 0
    let TSBufferSize = window;
    let fadeLength = TSBufferSize / mixFractional
    let ratio = float slice.Length / float outputsize
    if slice.Length = outputsize then
        slice
    else
        let fadeBuffer = Array.zeroCreate<int16> fadeLength
        let vlInc = 1.f / float32 fadeLength
        let mutable rampDown, rampUp = 1.f, 0.f
        let mutable arampDown, arampUp = 1.f, 0.f
        let output = Array.zeroCreate<int> outputsize
        let output2 = Array.zeroCreate<int16> outputsize
        let mutable percent = 0.
        let mutable pos = 0
        for i in 0 .. TSBufferSize .. outputsize - 1 do 
            percent <- float i / float outputsize
            pos <- int (float slice.Length * percent)
            pos <- pos + (pos % 2)
            if i = 0 then
                //Buffer.BlockCopy(slice, pos, output, i, TSBufferSize * 4);
                for y = 0 to TSBufferSize + fadeLength - 1 do
                    output.[i + y] <- int slice.[pos + y]
            else
                // Buffer.BlockCopy(slice, pos+fadeLength, output, i, TSBufferSize * 4)
                rampDown <- 1.f
                rampUp <- 0.f
                arampDown <- 1.f
                arampUp <- 0.f
                for y = 0 to TSBufferSize + fadeLength - 1 do
                    if i + y >= output.Length ||  pos + y >= slice.Length then 
                        ()
                    else
                        let oldData = int16 (float32 output.[i + y] * arampDown)
                        let newData = int16 (float32 slice.[pos + y] * arampUp)
                        let MergedData = int (oldData + newData)
                        output.[i + y] <- MergedData
                        if MergedData > MXVal then MXVal <- MergedData
                        if -MergedData > MXVal then MXVal <- -MergedData
                        if rampDown > 0.f then
                            rampDown <- rampDown - vlInc
                            if rampDown < 0.f then rampDown <- 0.f
                            // arampDown <- rampDown * (MathHelper.PiOver2)
                            arampDown <- -(rampDown * rampDown) + (2.f * rampDown)
                            if arampDown < 0.f then arampDown <- 0.f
                            if arampDown > 1.f then arampDown <- 1.f
                            // arampDown <- arampDown * 0.65f
                        if rampUp < 1.f then
                            rampUp <- rampUp + vlInc
                            if rampUp > 1.f then rampUp <- 1.f
                            //arampUp = rampUp * (MathHelper.PiOver2)
                            arampUp <- -(rampUp * rampUp) + (2.f * rampUp)
                            if arampUp > 1.f then arampUp <- 1.f
                            if arampUp < 0.f then arampUp <- 0.f
                            // arampUp <- arampUp * 0.65f
            if i < output.Length - TSBufferSize then
                //System.Diagnostics.Debug.WriteLine("Copying FadeBuffer");
                Buffer.BlockCopy(output, (i + TSBufferSize) - fadeLength, fadeBuffer, 0, fadeLength * 2)
        let limit = float32 Int16.MaxValue / float32 MXVal
        for i = 0 to output.Length - 1 do
            output2.[i] <- int16 (float32 output.[i] * limit)
        output2

