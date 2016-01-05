pipeline {
    let inputQ = new PipelineInputQueue<_>(this.QueueLength, 0)
    let! rawDataQ = stage this.GetRawData inputQ
    let! recordsQ = stage this.ParseRawData rawDataQ
    let! conformedRecordsQ = stage this.ConformRecords recordsQ
    let! transformedDataQ = stage this.TransformRecord conformedRecordsQ
    do! sink this.Sink transformedDataQ     
    return inputQ
}