let selectPeek (seqs: seq<_> seq) (probs: float seq)= 
    let r = System.Random()
    let enums = seqs  |> Seq.map (fun x-> x.GetEnumerator())  |> Seq.toArray
    let probs = probs |> Seq.toArray
    assert(enums.Length = probs.Length)
    //curlist always contains non empty sequences
    let curlist  = enums |> Array.mapi (fun i x-> i,x) 
                         |> Array.fold(fun s (i,x) -> if x.MoveNext() <> false then i::s else s ) List.empty

    
    let rec select curlist zleft = seq { //get next element and return list of future ids to peek from
        let rec next (curlist:int list) zleft = //walks prob tree and return next element 
            match curlist with
            |    [] -> None
            | i::[] -> Some(i, enums.[i].Current)
            | i::xs -> if r.NextDouble() < probs.[i]/zleft then 
                          Some(i, enums.[i].Current)
                       else
                          next xs (zleft-probs.[i])
        match next curlist zleft with
        | Some (i,e) -> yield e
                        let curlist', zleft' =  if enums.[i].MoveNext() <> false then curlist, zleft 
                                                else  let curlist' = curlist |> List.filter(fun e -> e <> i)
                                                      curlist', curlist' |> List.fold(fun s e -> s + probs.[e]) 0.
                        yield! select curlist' zleft'
        | _ -> ()
    }
    select curlist (curlist |> List.fold(fun s e -> s + probs.[e]) 0.)


let rec ones() = seq{ yield 1; yield! ones() }
let rec twos() = seq{ yield 2;yield! twos() }


let onetwos = selectPeek [ones(); twos()] [|2.;1.|] |> Seq.truncate 100 |> Seq.toArray

let none,ntwo  = onetwos |> Seq.filter(fun x -> x = 1) |> Seq.length  , onetwos |> Seq.filter(fun x -> x = 2) |> Seq.length  
