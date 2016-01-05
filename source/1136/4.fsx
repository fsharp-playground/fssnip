open System

type Board<'T> = 'T[,]

type X = int

type Y = int

type Position = X * Y

type PositionList = Position list 

type ProcessedPositions = PositionList

type ContiguousPoints = PositionList

type MassFinder = ContiguousPoints * ProcessedPositions

type Earth = 
    | Land
    | Water

let ViewType e = 
    match e with
        | Land -> "L"
        | Water -> "W"

let board = array2D [[Land;  Land;  Land;  Land;];
                     [Water; Land;  Land;  Water;];
                     [Land;  Water; Water; Water;];
                     [Water; Land;  Land;  Water;]]

let boardInt = array2D [[0;  0;  0;  1;];
                        [1; 0;  0;  1;];
                        [0;  1; 1; 1;];
                        [1; 0;  0;  1;]]
(* 
    Helper methods to move the position around
*)
                                          
let moveRight position = 
    let (x,y) = position
    (x + 1, y)

let moveLeft position = 
    let (x,y) = position
    (x - 1, y)

let moveUp position = 
    let (x,y) = position
    (x, y + 1)

let moveDown position = 
    let (x,y) = position
    (x, y - 1)

(*
    Size helpers
*)

let xSize board = Array2D.length1 board

let ySize board = Array2D.length2 board

let offBoard position board = 
    let (x,y) = position
    x < 0 || y < 0 || x >= (xSize board) || y >= (ySize board)

(*
    Alias to push elements onto a list
*)

let markPosition position previousSpots = position::previousSpots

(*
    Determines if the position on the board equals the target
*)

let positionOnTarget position board target = 
    if offBoard position board then 
        false
    else
        let (x, y) = position
        (Array2D.get board x y) = target

(*
    Alias to find if we already processed a position
*)

let positionExists position list = 
    List.exists(fun pos -> pos = position) list

(* 
   Iterate over each element in a 2d array, passing the x and y
   coordinate and the board, to the supplied function
   which can return an item. The items are all cons together
   and the function returns a new list
*)

let forEachElement (applier:(X -> Y -> Board<'a> -> 'b)) (twoDimArray:Board<'a>) =
    let mutable items = [] 
    for x in 0..(xSize board)-1 do
        for y in 0..(ySize board)-1 do            
            items <- (applier x y twoDimArray)::items
    items

let elemAt board (x, y) = Array2D.get board x y

(*
    Looks for a specified contigoius block
    and keeps track of processed positions using a 
    reference cell of a list of positions (supplied by the caller)
*)

         

let findMassStartingAt (position:Position) (board:Board<'A>) (target:'A) (positionSeed:ProcessedPositions) : MassFinder = 
    let rec findMassStartingAt' position (currentMass:ContiguousPoints, processedList:ProcessedPositions) cont = 
            // if you move off the board return
        if offBoard position board then
            cont (currentMass, processedList)

        // if you already processed this position then don't do anything
        else if positionExists position processedList then
            cont (currentMass, processedList)
        else  
            
            // branch out left, up, right, and down and see what you can find
            let up = moveUp position
            let down = moveDown position
            let left = moveLeft position
            let right = moveRight position
            
            let found = positionOnTarget position board target

            let updatedProcess = position::processedList

            match found with 
                | true ->                    
                           let massState = (position::currentMass, updatedProcess)

                           findMassStartingAt' up  massState (fun foundMassUp -> 
                           findMassStartingAt' down foundMassUp (fun foundMassDown ->
                           findMassStartingAt' left foundMassDown (fun foundMassLeft ->
                           findMassStartingAt' right foundMassLeft cont))) 

                | false -> 
                    // if you didn't find anything return the masses that you 
                    // found prevoiusly
                    cont((currentMass, updatedProcess))

    findMassStartingAt' position ([], positionSeed) id

(*
    Finds all items of list2 that are not in list1
*)

let except list1 list2 = 
    let listContainsElement item = List.exists (fun i -> i = item) list1
    List.filter(fun item -> not (listContainsElement item)) list2

(*
    Find first non processed position
*)

let firstNonProcessedPosition processedList xCount yCount = 
    match processedList with
        | [] -> 
            Some((0, 0))
        | _ ->
            if List.length processedList = (xCount * yCount) then
                None 
            else

                // get an array representing (x, y) tuples of the entire board
                let totalPositions = [0..xCount] |> List.collect (fun x -> [0..yCount] |> List.map (fun y -> (x, y)))

                // set intersections from the total positions array and the entire board
                let intersections = Set.intersect (Set.ofList totalPositions) (Set.ofList processedList)
                                        |> List.ofSeq

                // exclude the intersections from the total list
                let excludes = except intersections totalPositions

                match excludes with 
                    | [] -> None
                    | _ -> Some(List.head excludes)

                        

(*
    Finds all contiguous blocks of the specified type
    and returns a list of lists (each list is the points for a specific
    block)
*)
    
let getContiguousBlocks board target = 
    
    let xCount = (xSize board) - 1
    let yCount = (ySize board) - 1

    let rec findBlocks' (blocks, processed:PositionList) = 
        
        let findMass x y board = findMassStartingAt (x, y) board target processed

        // find the first non processed block 
        // and try and find its contigoius area
        // if it isn't a valid area the block it returns will be
        // empty and we can exclude it
        match firstNonProcessedPosition processed xCount yCount with 
            | None -> blocks
            | Some (x, y) -> 
                let (block, processed) = findMass x y board


                findBlocks' ((match block with 
                                | [] -> blocks
                                | _ -> block::blocks), processed)
        
    findBlocks' ([],[])

(*
    Returns a list of points representing a contigious block 
    of the type that the point was at. 
*)

let floodFillArea (point:Position) (canvas:Board<'T>) =
    let (x, y) = point
    let itemAtPoint = Array2D.get canvas x y
    
    findMassStartingAt point canvas itemAtPoint [] |> fst


(* 
    Test functions to run it
*)


let masses = getContiguousBlocks board Land

let largestList = List.maxBy(List.length) masses


System.Console.WriteLine("Largest mass is " + (List.length largestList).ToString());
