let rec iterate f value = seq { 
  yield value
  yield! iterate f (f value) }

// Returns: seq [1; 2; 4; 8; ...]
Seq.take 10 (iterate ((*)2) 1)