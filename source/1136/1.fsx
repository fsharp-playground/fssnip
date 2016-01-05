type Board<'T> = 'T[,]

type X = int

type Y = int

type Position = X * Y

type Processed = Position list ref

type Earth = 
    | Land
    | Water

let board = array2D [[Land;  Land;  Land;  Water;];
                     [Water; Land;  Land;  Water;];
                     [Land;  Water; Water; Water;];
                     [Water; Land;  Land;  Water;]]

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
    for x in 0..(xSize board) do
        for y in 0..(ySize board) do            
            items <- (applier x y twoDimArray)::items
    items


(*
    Looks for a specified contigoius block
    and keeps track of processed positions using a 
    reference cell of a list of positions (supplied by the caller)
*)

let findMassStartingAt (position:Position) (board:Board<'A>) (target:'A) (processed:Processed) = 
    let rec findMassStartingAt' position currentMass = 
            // if you move off the board return
        if offBoard position board then
            currentMass

        // if you already processed this position then don't do anything
        else if positionExists position !processed then
            currentMass
        else  
            
            // branch out left, up, right, and down and see what you can find
            let up = moveUp position
            let down = moveDown position
            let left = moveLeft position
            let right = moveRight position
            
            let found = positionOnTarget position board target

            processed := position::!processed;
            match found with 
                | true ->
                    position::currentMass  
                        |> findMassStartingAt' up 
                        |> findMassStartingAt' down 
                        |> findMassStartingAt' left 
                        |> findMassStartingAt' right 

                | false -> 
                    // if you didn't find anything return the masses that you 
                    // found prevoiusly
                    currentMass

    findMassStartingAt' position []

(*
    Finds all contiguous blocks of the specified type
    and returns a list of lists (each list is the points for a specific
    block)
*)

let getContiguousBlocks board target = 
    // keep track of all processed positions across the recursions
    let processed = ref []

    // go through each board element and find masses starting at the
    // the current position
    // filter out any positions that found no masses
    let massFinder x y board = findMassStartingAt (x, y) board target processed

    forEachElement massFinder board
        |> List.filter (fun i -> not (List.isEmpty i))

(*
    Returns a list of points representing a contigious block 
    of the type that the point was at. 
*)

let floodFillArea (point:Position) (canvas:Board<'T>) =
    let (x, y) = point
    let itemAtPoint = Array2D.get canvas x y
    let processed = ref []

    findMassStartingAt point canvas itemAtPoint processed



(* 
    Test functions to run it
*)

let masses = getContiguousBlocks board Land

let largestList = List.maxBy(List.length) masses

let massAt = floodFillArea (2, 2) board

System.Console.WriteLine("Largest mass is " + (List.length largestList).ToString());
