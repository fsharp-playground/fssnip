type Report = { 
    ID: int
    Amount: int 
}

 // Sample csv
let list = [|{ID = 4000; Amount = 8;}; {ID = 4001; Amount = 9;}; {ID = 3000; Amount = 5;};|]

let total = 
  list
  |> Seq.map (fun row -> row) 
  |> Seq.takeWhile (fun col -> (col.ID <= 4999) && (col.ID >= 4000) ) 
  
let totalSum = 
  total
  |> Seq.map(fun row -> row.Amount )
  |> Seq.sum  
