open System
open Deedle

#load "Deedle.fsx"

let sampleByCnt data s = 
    let r = new Random(0)
    let sampleSize = match ((data |> Seq.length) / 2 ) >= s with
                            | false  -> (data |> Seq.length) / 2 
                            | true -> s
        
    let rndData = data |> Seq.map(fun d-> (d,r.Next()))
                    |> Seq.sortBy(snd)
                    |> Seq.map(fst)
                    |> Seq.toList
    (rndData |> Seq.take sampleSize |> Seq.toList), 
    (rndData |> Seq.skip sampleSize |> Seq.take sampleSize  |> Seq.toList)
         
let sampleByRatio data ratio =      
    let total = data |> Seq.length
    let first = (int) (Math.Round((float)(total) * ratio))
    let second = total - first 
    let r = new Random(0)
    let rndData = data |> Seq.map(fun d-> (d,r.Next()))
                    |> Seq.sortBy(snd)
                    |> Seq.map(fst)
                    |> Seq.toList
    (rndData |> Seq.take first |> Seq.toList), 
    (rndData |> Seq.skip first |> Seq.take second  |> Seq.toList)         
           


let sampleFrameByCnt (df:Frame<int,string>) (cnt:int) =
    let keySample = sampleByCnt df.RowKeys cnt
    (df |> Frame.getRows (fst keySample)),(df |> Frame.getRows (snd keySample))


let sampleFrameByRatio (df:Frame<int,string>) (ratio:float) =
    let keySample = sampleByRatio df.RowKeys ratio
    (df |> Frame.getRows (fst keySample)),(df |> Frame.getRows (snd keySample))
