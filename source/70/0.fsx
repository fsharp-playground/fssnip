let rec iterate f value = seq { yield value; yield! iterate f (f value) }

Seq.take 10 (iterate ((*)2) 1) // seq [1; 2; 4; 8; ...]