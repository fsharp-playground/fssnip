type State =  Up 
             |Down
let datass = [
                [(Up,90);(Down,2);(Up,20);(Down,3);(Up,10);(Down,4);(Up,100);(Down,11);(Up,10);(Down,30)]
                [(Up,100);(Down,4);(Up,10);(Down,9);(Up,2);(Down,20);(Up,300);(Down,11);(Up,30);(Down,30)]
                [(Down,9);(Up,20);(Down,20);(Up,10);(Down,4);(Up,10);(Up,300);(Down,11);(Up,30);(Down,30)]
                [(Up,90);(Down,2);(Up,10);(Down,4);(Up,10);(Down,11);(Up,200);(Down,3);(Up,10);(Down,30)]
                [(Up,100);(Down,4);(Up,10);(Down,9);(Up,30);(Down,30);(Up,200);(Down,20);(Up,3);(Down,11)]
                [(Up,30);(Down,11);(Up,30);(Down,30);(Down,9);(Up,2);(Down,20);(Up,400);(Down,4);(Up,10)] 
                [(Up,100);(Down,4);(Up,10);(Down,11);(Up,9);(Down,2);(Up,200);(Down,3);(Up,10);(Down,30)]
                [(Up,100);(Down,9);(Up,2);(Down,20);(Up,3);(Down,11);(Up,100);(Down,4);(Up,30);(Down,30)]
                [(Down,9);(Up,200);(Up,3);(Down,11);(Up,30);(Down,30);(Down,20);(Up,400);(Down,4);(Up,10)]                                 
                    ]
//percent dont make too much sense but were more interesting.
let toPercent (ds: (State*int) list) = 
    ds |> List.map snd
       |> List.fold (fun (e,rs) i -> (e+i, (e+i)::rs )) (0,[])
       |> fun (total, timePoints) -> timePoints |> List.map (fun e -> float e / float total * 100.)
       |> List.rev
       |> List.zip (ds |> List.map fst)   

let getStatusAt e (ds: (State*float) list) = 
    let ind = ds |> List.findIndex (fun n -> snd n >= e)
    ds.Item(ind) |> fst

let allPercents = datass |> List.map toPercent

let allSplits = allPercents
                        |> List.fold (fun acc ele -> acc @ ele) []
                        |> List.map snd
                        |> List.sort

let toAllSplits (splices: float list) (ds: (State*float) list) = 
    splices |> List.fold (fun acc spl -> (getStatusAt spl ds) :: acc) []
            |> List.rev
 
let allSplitStates = allPercents |> List.map (fun e -> toAllSplits allSplits e)

let totalUptime =
    let allUps = [0..(allSplits.Length - 1)]
    let filterUps (stas: State list) (upInds: int list) = upInds |> List.filter (fun e -> stas.Item e = Up)
    let Ups = allSplitStates |> List.fold (fun acc ele -> filterUps ele acc) allUps
    allUps |> List.mapi (fun i e-> if Ups |> List.exists (fun x -> i = x) then Up
                                                                          else Down) |> List.zip allSplits

do  printfn "%A" totalUptime
   
do System.Console.ReadKey() |> ignore