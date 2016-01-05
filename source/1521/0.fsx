open Nessos.Streams
let data = [|1..10000000|] |> Array.map int64

data 
|> Stream.ofArray 
|> Stream.filter (fun x -> x % 2L = 0L)  
|> Stream.map (fun x -> x + 1L) 
|> Stream.sum
//Real: 00:00:00.290, CPU: 00:00:00.280, GC gen0: 0, gen1: 0, gen2: 0
//val it : int64 = 25000010000000L

data 
|> Stream.ofArray 
|> Stream.filter (fun x -> x % 2L = 0L)  
|> Stream.map (fun x -> x + 1L) 
|> Stream.sum;;
//Real: 00:00:00.286, CPU: 00:00:00.280, GC gen0: 0, gen1: 0, gen2: 0
//val it : int64 = 25000010000000L

data 
|> Stream.ofArray 
|> Stream.filter (fun x -> x % 2L = 0L)  
|> Stream.map (fun x -> x + 1L) 
|> Stream.sum
//Real: 00:00:00.287, CPU: 00:00:00.296, GC gen0: 0, gen1: 0, gen2: 0
//val it : int64 = 25000010000000L

data 
|> Array.filter (fun x -> x % 2L = 0L)  
|> Array.map (fun x -> x + 1L) 
|> Array.sum
//Real: 00:00:00.283, CPU: 00:00:00.280, GC gen0: 1, gen1: 1, gen2: 1
//val it : int64 = 25000010000000L

data 
|> Array.filter (fun x -> x % 2L = 0L)  
|> Array.map (fun x -> x + 1L) 
|> Array.sum
//Real: 00:00:00.276, CPU: 00:00:00.280, GC gen0: 1, gen1: 1, gen2: 1
//val it : int64 = 25000010000000L

data 
|> Array.filter (fun x -> x % 2L = 0L)  
|> Array.map (fun x -> x + 1L) 
|> Array.sum
//Real: 00:00:00.251, CPU: 00:00:00.249, GC gen0: 0, gen1: 0, gen2: 0
//val it : int64 = 25000010000000L

//The values I see doesnt match with the ones posted on the f# streams page.
//Am I missing something? 