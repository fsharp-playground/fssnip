let isFizzBuzz x = 
        (x % 3 = 0) || (x % 5 = 0)

let toFizzBuzz x =
        match (x % 3,x % 5) with
        | (0,0) -> "fizzbuzz"
        | (0,_) -> "fizz"
        | (_,0) -> "buzz"
        | _ ->  ""

let fizzBuzzCandidates =
        seq {1 .. System.Int32.MaxValue } 
        |> Seq.filter(isFizzBuzz) 
        

let fizzBuzzList start length =
        fizzBuzzCandidates
        |> Seq.skip start
        |> Seq.take length   

let rec InverseFizzBuzz  (index:int) (fizzBuzzInput:list<string>) = 
        let fizzBuzzNumbers  = fizzBuzzList index fizzBuzzInput.Length                   
        let fizzBuzzStrings = fizzBuzzNumbers |> Seq.map(toFizzBuzz)                      
        if (Seq.forall2(fun x y -> x = y) fizzBuzzStrings fizzBuzzInput) then
            fizzBuzzNumbers
        else
            InverseFizzBuzz (index + 1) fizzBuzzInput

let GetInverseFizzBuzz (fizzBuzzInput:list<string>) = 
        InverseFizzBuzz 0 fizzBuzzInput     

